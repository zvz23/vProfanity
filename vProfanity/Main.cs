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
using System.Threading.Tasks;
using System.Net.Http;

namespace vProfanity
{
    public partial class Main : Form
    {

        private readonly List<WordOption> detectedWords = new List<WordOption>();
        private readonly List<SexualOption> detectedSexualImagesTimes = new List<SexualOption>();
        private readonly CheckedListBox audioListBox;
        private readonly CheckedListBox videoListBox;
        public Main()
        {
            InitializeComponent();
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
            audioListBox.SelectedIndexChanged += audioListBox_SelectedIndexChanged;
            tabControl1.TabPages[0].Text = "Audio";
            tabControl1.TabPages[0].Controls.Add(audioListBox);
            tabControl1.TabPages[1].Text = "Video";
            tabControl1.TabPages[1].Controls.Add(videoListBox);


        }

        
        private void _setDefaultControlState()
        {

            audioListBox.BeginUpdate();
            audioListBox.Items.Clear();
            audioListBox.EndUpdate();

            videoListBox.BeginUpdate();
            videoListBox.Items.Clear();
            videoListBox.EndUpdate();

            detectedSexualImagesTimes.Clear();
            detectedWords.Clear();


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
                WordOption word = (WordOption)listBox1.SelectedItem;
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

        private void loadWordsFromJson(string transcriptJson)
        {
            List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(transcriptJson);
            AppDBContext appDBContext = new AppDBContext();
            foreach (var t in transcript)
            {
                string[] words = t.text.Split(' ');

                foreach (var word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        WordOption WordOption = new WordOption()
                        {
                            Word = word,
                            StartTime = t.start,
                            EndTime = t.end,
                            IsProfane = appDBContext.IsProfane(word)
                        };
                        detectedWords.Add(WordOption);
                    }
                }

            }
        }

        public void loadDetectedSexualFromJson(string detectedSexualFrameTimesJson)
        {
            List<SexualFrameTime> detectedSexualFrameTimes = JsonConvert.DeserializeObject<List<SexualFrameTime>>(detectedSexualFrameTimesJson);
            foreach (var t in detectedSexualFrameTimes) 
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(t.Milliseconds);
                string durationString = duration.ToString(@"hh\:mm\:ss");

                detectedSexualImagesTimes.Add(new SexualOption
                {
                    DurationFormat = durationString,
                    StartTime = t.Seconds
                });
            }
        }

        private void censorVideo(string videoHash)
        {
            AppDBContext appDBContext = new AppDBContext();
            string detectedSexualTimesJson = appDBContext.GetDetectedSexualTimes(videoHash);

            List<SexualFrameTime> sexualFrameTimes = JsonConvert.DeserializeObject<List<SexualFrameTime>>(detectedSexualTimesJson);


            using (Py.GIL())
            {
                PyList sexualFrameTimesPyList = new PyList();
                foreach (var t in sexualFrameTimes)
                {
                    PyObject sexualFrameTimeDict = new PyDict();
                    sexualFrameTimeDict.SetItem("Milliseconds", new PyFloat(t.Milliseconds));
                    sexualFrameTimeDict.SetItem("Seconds", new PyFloat(t.Seconds));
                    sexualFrameTimeDict.SetItem("NextSeconds", new PyFloat(t.NextSeconds));
                    sexualFrameTimesPyList.Append(sexualFrameTimeDict);
                }
                dynamic censor_second = Py.Import("censor_second");
                dynamic result = censor_second.censor_video(axWindowsMediaPlayer1.URL, sexualFrameTimesPyList, AppConstants.CENSORED_VIDEO_OUTPUT_FOLER);
                MessageBox.Show($"The censored file is saved at {result.ToString()}");
            }

        }
        private void scanVideo(string videoHash)
        {
            List<FrameInfo> frameInfoList = null;
            using (Py.GIL())
            {
                dynamic image_exporter = Py.Import("image_exporter");
                dynamic frames_info_json = image_exporter.export_video_images_by_keyframes(axWindowsMediaPlayer1.URL, videoHash);
                frameInfoList = JsonConvert.DeserializeObject<List<FrameInfo>>(frames_info_json.ToString());

            }
            List<SexualFrameTime> sexualFrameTimes = new List<SexualFrameTime>();

            foreach (var frameInfo in frameInfoList)
            {
                var image = File.ReadAllBytes(frameInfo.FilePath);
                var input = new ModelInput() { ImageSource = image };
                var result = VProfanityModel.Predict(input);
                if (result.PredictedLabel == "2")
                {

                    TimeSpan duration = TimeSpan.FromMilliseconds(frameInfo.Milliseconds);
                    string durationString = duration.ToString(@"hh\:mm\:ss");
                    
                    detectedSexualImagesTimes.Add(new SexualOption
                    {
                        DurationFormat = durationString,
                        StartTime = frameInfo.Seconds
                    });
                    sexualFrameTimes.Add(new SexualFrameTime
                    {
                        Milliseconds = frameInfo.Milliseconds,
                        Seconds = frameInfo.Seconds,
                        NextSeconds = frameInfo.NextSeconds
                    });
                }
            }
            if (sexualFrameTimes.Count > 0)
            {
                AppDBContext appDBContext = new AppDBContext();
                string sexualFrameTimesJson = JsonConvert.SerializeObject(sexualFrameTimes);
                appDBContext.SaveDetectedSexualTimes(videoHash, sexualFrameTimesJson);
            }

        }

        private void scanAudio(string videoHash) 
        {
            using(Py.GIL())
            {
                dynamic speechtotext = Py.Import("speechtotext");
                dynamic transcript_json = speechtotext.speech_to_text(axWindowsMediaPlayer1.URL);
                string transcript_json_string = transcript_json.ToString();
                List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(transcript_json_string);
                if (transcript.Count > 0)
                {
                    var appDBContext = new AppDBContext();
                    appDBContext.SaveTranscript(videoHash, transcript_json_string);
                    loadWordsFromJson(transcript_json_string);
                }

            }

        }

        private void loadFromDb(string videoHash)
        {
            _setDefaultControlState();
            AppDBContext appDBContext = new AppDBContext();
            string transcriptJson = appDBContext.GetTranscript(videoHash);
            if (transcriptJson != null)
            {
                loadWordsFromJson(transcriptJson);

            }

            string detectedSexualTimesJson = appDBContext.GetDetectedSexualTimes(videoHash);
            if (detectedSexualTimesJson != null)
            {
                loadDetectedSexualFromJson(detectedSexualTimesJson);
            }
            
        }
        private async void scanButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(axWindowsMediaPlayer1.URL))
            {
                MessageBox.Show("Please upload video");
                return;
            }
            string videoHash = FileHashGenerator.GetFileHash(axWindowsMediaPlayer1.URL);
            AppDBContext appDBContext = new AppDBContext();

            if (appDBContext.HasRecord(videoHash))
            {
                DialogResult result = MessageBox.Show("There was a scan performed before, do you want to reuse its data?", "Information", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    _setDefaultControlState();

                    string transcriptJson = appDBContext.GetTranscript(videoHash);
                    if (transcriptJson != null)
                    {
                        audioListBox.BeginUpdate();
                        loadWordsFromJson(transcriptJson);
                        audioListBox.Items.AddRange(detectedWords.ToArray());
                        audioListBox.EndUpdate();
                    }
                    string detectedSexualTimesJson = appDBContext.GetDetectedSexualTimes(videoHash);
                    if(detectedSexualTimesJson != null)
                    {
                        videoListBox.BeginUpdate();
                        loadDetectedSexualFromJson(detectedSexualTimesJson);
                        videoListBox.Items.AddRange(detectedSexualImagesTimes.ToArray());
                        videoListBox.EndUpdate();
                    }
                    return;
                }
            }

            _setDefaultControlState();

            uploadButton.Enabled = false;
            scanButton.Text = "Scanning";
            scanButton.Enabled = false;
            censorButton.Enabled = false;
            exportButton.Enabled = false;
            await Task.Run(() => scanVideo(videoHash));
            await Task.Run(() => scanAudio(videoHash));
            if (detectedSexualImagesTimes.Count > 0)
            {
                videoListBox.BeginUpdate();
                videoListBox.Items.Clear();
                videoListBox.Items.AddRange(detectedSexualImagesTimes.ToArray());
                videoListBox.EndUpdate();
            }
            if (detectedWords.Count > 0)
            {
                audioListBox.BeginUpdate();
                audioListBox.Items.Clear();
                audioListBox.Items.AddRange(detectedWords.ToArray());
                audioListBox.EndUpdate();
            }
 

            uploadButton.Enabled = true;
            scanButton.Text = "Scan";
            scanButton.Enabled = true;
            censorButton.Enabled = true;
            exportButton.Enabled = true;
            MessageBox.Show("The scan has finished.");

        }

        private void _loadWords(List<TranscriptChunk> transcript)
        {
            detectedWords.Clear();
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            AppDBContext appDBContext = new AppDBContext();
            foreach (var t in transcript)
            {
                string[] words = t.text.Split(' ');
                
                foreach (var word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        WordOption WordOption = new WordOption()
                        {
                            Word = word,
                            StartTime = t.start,
                            EndTime = t.end,
                            IsProfane = appDBContext.IsProfane(word)
                        };
                        detectedWords.Add(WordOption);
                        
                    }
                }
                listBox1.Items.AddRange(detectedWords.ToArray());
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
                stringBuilder.Append($" {t.text}");
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
                WordOption[] searchResult = detectedWords.Where(w => w.Word.ToLower().Contains(searchBox.Text.ToLower())).ToArray();
                listBox1.Items.AddRange(searchResult);
            }
            else
            {
                listBox1.Items.AddRange(detectedWords.ToArray());
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
                extractForm.SelectedItem = (WordOption)listBox1.SelectedItem;
                extractForm.VideoURL = axWindowsMediaPlayer1.URL;
                extractForm.ShowDialog(this);
            }


        }

        private void filterComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            audioListBox.BeginUpdate();
            audioListBox.Items.Clear();
            if (filterComboBox.Text == "Profane")
            {
                WordOption[] searchResult = detectedWords.Where(w => w.IsProfane).ToArray();
                audioListBox.Items.AddRange(searchResult);
            }
            else
            {
                audioListBox.Items.AddRange(detectedWords.ToArray());
            }

            audioListBox.EndUpdate();
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

        private void audioListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (audioListBox.SelectedItem != null)
            {
                WordOption wordOption = (WordOption)audioListBox.SelectedItem;
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = wordOption.StartTime;
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
            using(var form = new DependencyDownloaderForm())
            {
                form.Show(this);
            }
        }

        private async void censorButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(axWindowsMediaPlayer1.URL))
            {
                MessageBox.Show("Please upload video");
                return;
            }

            uploadButton.Enabled = false;
            scanButton.Enabled = false;
            censorButton.Enabled = false;
            censorButton.Text = "Censoring";
            exportButton.Enabled = false;

            string videoHash = FileHashGenerator.GetFileHash(axWindowsMediaPlayer1.URL);
            AppDBContext appDBContext = new AppDBContext();
            string detectedSexualTimesJson = appDBContext.GetDetectedSexualTimes(videoHash);
            if (detectedSexualTimesJson == null)
            {
                MessageBox.Show("There is no sexual conted detected.");
                return;
            }



            await Task.Run(() => censorVideo(videoHash));

            uploadButton.Enabled = true;
            scanButton.Enabled = true;
            censorButton.Enabled = true;
            censorButton.Text = "Censor";
            exportButton.Enabled = true;
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            PythonEngine.Shutdown();

        }

        private void Main_Shown(object sender, EventArgs e)
        {
            using (var form = new DependencyDownloaderForm())
            {
                form.ShowDialog(this);
            }
        }
    }


    public class WordOption
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

    public class TranscriptChunk
    {
        public double start { get; set; }
        public double end { get; set; }
        public string text { get; set; }
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

    public class FrameInfo
    {
        public string FilePath { get; set; }
        public double Milliseconds { get; set; }
        public double Seconds { get; set; }
        public double NextSeconds { get; set; }
    }

    public class SexualFrameTime
    {
        public double Milliseconds { get; set; }
        public double Seconds { get; set; }
        public double NextSeconds { get; set; }

    }

}
