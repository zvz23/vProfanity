using vProfanity.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Net;
using System.Drawing;
using static vProfanity.VProfanityModel;
using Microsoft.ML;
using Python.Runtime;

namespace vProfanity
{
    public partial class Main : Form
    {

        public List<WordClass> LastWords = new List<WordClass>();
        private readonly CheckedListBox audioListBox;
        private readonly CheckedListBox videoListBox;
        public Main()
        {
            InitializeComponent();
            filterComboBox.SelectedIndex = 0;
            audioListBox = new CheckedListBox()
            {
                Width = tabControl1.Width,
                Height = tabControl1.Height,
                BorderStyle = BorderStyle.None
            };

            videoListBox = new CheckedListBox()
            {
                Width = tabControl1.Width,
                Height = tabControl1.Height,
                BorderStyle = BorderStyle.None,
            };
            videoListBox.SelectedIndexChanged += videoListBox_SelectedIndexChanged;
            tabControl1.TabPages[0].Text = "Audio";
            tabControl1.TabPages[0].Controls.Add(audioListBox);
            tabControl1.TabPages[1].Text = "Video";
            tabControl1.TabPages[1].Controls.Add(videoListBox);
            prepareTab();


        }

        private void prepareTab()
        {
            //string[] mySpeech = { "the", "person", "go", "outside", "punch", "yourself", "go", "to", "town", "lorem", "ipsum", "dolor", "why", "I", "can't", "understand", "lorem", "ipsum", "but", "I", "can", "understand", "pig", "ipsum" };
            //List<string> sexualRegions = new List<string>();
            //string[] myVideoRegions = { "00:00:23", "00:03:24", "00:10:05", "00:00:23", "00:00:23", "00:00:23", "00:00:23", "00:00:23", "00:00:23", "00:00:23" };
            //audioListBox.Items.AddRange(mySpeech);
            //videoListBox.Items.AddRange(sexualRegions.ToArray());


        }

        private void _loadAppDependencies()
        {
            using(var webClient = new WebClient())
            {
            }
        }
       
        private void _processExited(object sender, System.EventArgs e)
        {
            string fileName = Path.GetFileNameWithoutExtension(axWindowsMediaPlayer1.URL);
            string jsonFilePath = Path.Combine(AppConstants.ABS_TEMP_FOLDER, $"{fileName}.en.json");
            if (File.Exists(jsonFilePath)) 
            {
                string videoHash = FileHashGenerator.GetFileHash(axWindowsMediaPlayer1.URL);
                var dbContext = new AppDBContext();
                string hasTranscript = dbContext.GetTranscript(videoHash);
                string transcriptJson = File.ReadAllText(jsonFilePath);
                if (hasTranscript == null)
                {
                    dbContext.SaveTranscript(videoHash, transcriptJson);
                }
                else
                {
                    dbContext.UpdateTranscript(videoHash, transcriptJson);
                }
                
                List<TranscriptChunk> transcript = TranscriptSerializer.DeserializeFile(jsonFilePath);
                this.Invoke(new MethodInvoker(delegate
                {
                    _loadWords(transcript);
                }));

            }
            this.Invoke(new MethodInvoker(delegate
            {
                scanButton.Text = "Scan";
                scanButton.Enabled = true;
                uploadButton.Enabled = true;
                MessageBox.Show("The scan has completed");
            }));
            
        }

        private void _setDefaultControlState()
        {
            audioListBox.BeginUpdate();
            audioListBox.Items.Clear();
            audioListBox.EndUpdate();

            videoListBox.BeginUpdate();
            videoListBox.Items.Clear();
            videoListBox.EndUpdate();

            uploadButton.Enabled = true;
            scanButton.Enabled = true;
            scanButton.Text = "Scan";
            censorButton.Enabled = true;
            analyzeButton.Enabled = true;
            toxicityValueLabel.Text = "N/A";
            identityAttackValueLabel.Text = "N/A";
            insultValueLabel.Text = "N/A";
            profanityValueLabel.Text = "N/A";
            threatValueLabel.Text = "N/A";
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                WordClass word = (WordClass)listBox1.SelectedItem;
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = word.StartTime;
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void uploadButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                openFileDialog.Filter = "Video Files(*.mp4)|*.mp4";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    axWindowsMediaPlayer1.URL = openFileDialog.FileName;
                    _setDefaultControlState();
                }
            }
        }

        private void scanButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(axWindowsMediaPlayer1.URL))
            {
                MessageBox.Show("Please upload video");
                return;
            }
            uploadButton.Enabled = false;
            scanButton.Text = "Scanning";
            scanButton.Enabled = false;
            censorButton.Enabled = false;
            exportButton.Enabled = false;

            string videoHash = FileHashGenerator.GetFileHash(axWindowsMediaPlayer1.URL);
            string images_path = string.Empty;
            using (Py.GIL())
            {
                dynamic image_exporter = Py.Import("image_exporter");
                dynamic images_path_dynamic = image_exporter.export_video_images_by_second(axWindowsMediaPlayer1.URL, videoHash);
                images_path = images_path_dynamic.ToString();

            }
            List<SexualOption> detectedSexualImagesTime = new List<SexualOption>();

            foreach (var file in Directory.EnumerateFiles(images_path))
            {
                var image = File.ReadAllBytes(file);
                var input = new ModelInput() { ImageSource = image };
                var result = VProfanityModel.Predict(input);
                if (result.PredictedLabel == "2")
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    int fileNameInt = int.Parse(fileName);
                    TimeSpan duration = TimeSpan.FromSeconds(fileNameInt);
                    string durationString = duration.ToString(@"hh\:mm\:ss");

                    detectedSexualImagesTime.Add(new SexualOption
                    {
                        DurationFormat = durationString,
                        StartTime = double.Parse(fileName)
                    });
                }
            }
            if (detectedSexualImagesTime.Count > 0)
            {
                videoListBox.BeginUpdate();
                videoListBox.Items.AddRange(detectedSexualImagesTime.ToArray());
                videoListBox.EndUpdate();
            }
            uploadButton.Enabled = true;
            scanButton.Text = "Scan";
            scanButton.Enabled = true;
            censorButton.Enabled = true;
            exportButton.Enabled = true;
            MessageBox.Show("The scan has finished.");
            //var dbContext = new AppDBContext();
            //string hasTranscript = dbContext.GetTranscript(videoHash);
            //if (hasTranscript != null)
            //{
            //    DialogResult result = MessageBox.Show("There was a scan performed before, do you want to reuse its data?", "Information", MessageBoxButtons.YesNo);
            //    if (result == DialogResult.Yes)
            //    {
            //        List<TranscriptChunk> transcript = TranscriptSerializer.Deserialize(hasTranscript);
            //        _loadWords(transcript);
            //        return;
            //    }
            //}

            //scanButton.Text = "Scanning";
            //scanButton.Enabled = false;
            //uploadButton.Enabled = false;
            //var transcriptGenerator = new TranscriptGenerator();
            //string fileName = $"{Path.GetFileNameWithoutExtension(axWindowsMediaPlayer1.URL)}.en.json";
            //if (File.Exists(Path.Combine(AppConstants.ABS_TEMP_FOLDER, fileName)))
            //{
            //    File.Delete(Path.Combine(AppConstants.ABS_TEMP_FOLDER, fileName));
            //}
            //Process process = transcriptGenerator.GenerateTranscriptProcess(axWindowsMediaPlayer1.URL);
            //process.Exited += new EventHandler(_processExited);
            //process.Start();
        }

        private void _loadWords(List<TranscriptChunk> transcript)
        {
            LastWords.Clear();
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            AppDBContext appDBContext = new AppDBContext();
            foreach (var t in transcript)
            {
                string[] words = t.content.Split(' ');
                
                foreach (var word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        WordClass wordClass = new WordClass()
                        {
                            Word = word,
                            StartTime = t.start,
                            EndTime = t.end,
                            IsProfane = appDBContext.IsProfane(word)
                        };
                        LastWords.Add(wordClass);
                        
                    }
                }
                listBox1.Items.AddRange(LastWords.ToArray());
                listBox1.EndUpdate();
            }
        }

        private async void analyzeButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(axWindowsMediaPlayer1.URL))
            {
                MessageBox.Show("Please upload and scan a video");
                return;
            }
            string videoHash = FileHashGenerator.GetFileHash(axWindowsMediaPlayer1.URL);
            AppDBContext dbContext = new AppDBContext();

            string rawTranscript = dbContext.GetTranscript(videoHash);
            if (rawTranscript == null)
            {
                MessageBox.Show("Please scan a video");
                return;
            }
            analyzeButton.Text = "Analyzing";
            analyzeButton.Enabled = false;
            List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(rawTranscript);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var t in transcript)
            {
                stringBuilder.Append($" {t.content}");
            }
            PerspectiveAPI perspectiveAPI = new PerspectiveAPI();
            ScoreResponse scoreResponse = await perspectiveAPI.AnaylizeText(stringBuilder.ToString());
            toxicityValueLabel.Text = $"{scoreResponse.attributeScores.TOXICITY.summaryScore.value[2]} of 10 people";
            identityAttackValueLabel.Text = $"{scoreResponse.attributeScores.IDENTITY_ATTACK.summaryScore.value[2]} of 10 people";
            insultValueLabel.Text = $"{scoreResponse.attributeScores.INSULT.summaryScore.value[2]} of 10 people";
            profanityValueLabel.Text = $"{scoreResponse.attributeScores.PROFANITY.summaryScore.value[2]} of 10 people";
            threatValueLabel.Text = $"{scoreResponse.attributeScores.THREAT.summaryScore.value[2]} of 10 people";
            analyzeButton.Text = "Analyze Transcript";
            analyzeButton.Enabled = true;
        }

        private void searchBox_TextChanged(object sender, EventArgs e)
        {
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            if (!string.IsNullOrWhiteSpace(searchBox.Text))
            {
                WordClass[] searchResult = LastWords.Where(w => w.Word.ToLower().Contains(searchBox.Text.ToLower())).ToArray();
                listBox1.Items.AddRange(searchResult);
            }
            else
            {
                listBox1.Items.AddRange(LastWords.ToArray());
            }

            listBox1.EndUpdate();
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Please select a word from the list");
                return;
            }
            using (ExtractForm extractForm = new ExtractForm())
            {
                extractForm.SelectedItem = (WordClass)listBox1.SelectedItem;
                extractForm.VideoURL = axWindowsMediaPlayer1.URL;
                extractForm.ShowDialog(this);
            }


        }

        private void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            if (filterComboBox.Text == "Profane")
            {
                WordClass[] searchResult = LastWords.Where(w => w.IsProfane).ToArray();
                listBox1.Items.AddRange(searchResult);
            }
            else
            {
                listBox1.Items.AddRange(LastWords.ToArray());
            }

            listBox1.EndUpdate();
        }

        private void videoListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (videoListBox.SelectedItem != null)
            {
                SexualOption sexualOption = (SexualOption)videoListBox.SelectedItem;
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = sexualOption.StartTime;
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (AddVideoRegionForm form = new AddVideoRegionForm())
            {
                form.ShowDialog(this);
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void censorButton_Click(object sender, EventArgs e)
        {
            var image = File.ReadAllBytes(@"D:\School\Thesis\Dataset\vivamax\0eba56f7-b5cf-4a0d-96dd-9bda393c32e9_153.jpg");
            var input = new ModelInput() { ImageSource = image };
            var result = VProfanityModel.Predict(input);
            MessageBox.Show(result.PredictedLabel);
        }
    }

    public class WordClass
    {
        public string Word { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public bool IsProfane { get; set; } = false;


        public override string ToString()
        {
            return Word;
        }
    }

    public class SexualOption
    {
        public string DurationFormat { get; set; }
        public double StartTime { get; set; }

        public override string ToString()
        {
            return DurationFormat;
        }
    }
}
