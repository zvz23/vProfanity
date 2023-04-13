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

        private readonly List<WordOption> wordsOptions = new List<WordOption>();
        private readonly List<SexualOption> sexualOptions = new List<SexualOption>();
        private string currentFileHash;
        private readonly CheckedListBox audioListBox;
        private readonly CheckedListBox videoListBox;
        public Main()
        {
            InitializeComponent();
            audioListBox = new CheckedListBox()
            {
                Width = tabControl1.Width,
                Height = tabControl1.Height,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
            };

            videoListBox = new CheckedListBox()
            {
                Width = tabControl1.Width,
                Height = tabControl1.Height,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill
            };
            videoListBox.SelectedIndexChanged += videoListBox_SelectedIndexChanged;
            audioListBox.SelectedIndexChanged += audioListBox_SelectedIndexChanged;
            tabControl1.TabPages[0].Text = "Audio";
            tabControl1.TabPages[0].Controls.Add(audioListBox);
            tabControl1.TabPages[1].Text = "Video";
            tabControl1.TabPages[1].Controls.Add(videoListBox);


        }

        private Dictionary<string, List<List<double>>> getSegmentsFromListBox()
        {
            Dictionary<string, List<List<double>>> segmentsContainer = new Dictionary<string, List<List<double>>>();
            segmentsContainer["video"] = new List<List<double>>();
            segmentsContainer["audio"] = new List<List<double>>();

            if (videoListBox.CheckedItems.Count > 0)
            {
                foreach (var option in videoListBox.CheckedItems)
                {
                    SexualOption sexualOption = (SexualOption)option;
                    List<double> segment = new List<double>()
                    {
                        sexualOption.StartTime,
                        sexualOption.EndTime
                    };
                    segmentsContainer["video"].Add(segment); 
                }
            }

            if (audioListBox.CheckedItems.Count > 0)
            {
                foreach (var option in audioListBox.CheckedItems)
                {
                    WordOption wordOption = (WordOption)option;
                    List<double> segment = new List<double>()
                    {
                        wordOption.StartTime,
                        wordOption.EndTime
                    };
                    segmentsContainer["audio"].Add(segment);
                }
            }

            if (segmentsContainer["video"] == null && segmentsContainer["audio"] == null)
            {
                return null;
            }

            return segmentsContainer;

        }


        private void _setDefaultControlState()
        {

            audioListBox.BeginUpdate();
            audioListBox.Items.Clear();
            audioListBox.EndUpdate();

            videoListBox.BeginUpdate();
            videoListBox.Items.Clear();
            videoListBox.EndUpdate();

            sexualOptions.Clear();
            wordsOptions.Clear();

            uploadButton.Enabled = true;
            scanButton.Enabled = true;
            censorButton.Enabled = true;
            exportButton.Enabled = true;
            analyzeButton.Enabled = true;

            uploadButton.Text = "Upload";
            scanButton.Text = "Scan";
            censorButton.Text = "Censor";
            exportButton.Text = "Export";
            analyzeButton.Text = "Analyze Transcript";

            toxicityValueLabel.Text = "N/A";
            identityAttackValueLabel.Text = "N/A";
            insultValueLabel.Text = "N/A";
            profanityValueLabel.Text = "N/A";
            threatValueLabel.Text = "N/A";
        }


        private void uploadButton_Click(object sender, EventArgs e)
        {
            uploadButton.Enabled = false;
            uploadButton.Text = "Uploading";
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                openFileDialog.Filter = "Video Files(*.mp4)|*.mp4";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    axWindowsMediaPlayer1.URL = openFileDialog.FileName;
                    currentFileHash = FileHashGenerator.GetFileHash(axWindowsMediaPlayer1.URL);
                    _setDefaultControlState();
                }
            }
            uploadButton.Enabled = true;
            uploadButton.Text = "Upload";
        }

        private void loadWordsFromTranscriptJson(string transcriptJson)
        {
            List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(transcriptJson);
            AppDBContext appDBContext = new AppDBContext();
            foreach (var t in transcript)
            {
                string[] words = t.text.Split(' ');
                
                List<WordOption> wordOptions = words.Where(word => !string.IsNullOrWhiteSpace(word)).Select(word => new WordOption
                {
                    Word = word,
                    StartTime = t.start,
                    EndTime = t.end,
                    IsProfane = !t.hasProfane ? false : appDBContext.IsProfane(word)
                }).ToList();
                wordsOptions.AddRange(wordOptions);
                   
            }

            audioListBox.BeginUpdate();
            foreach (var option in wordsOptions)
            {
                audioListBox.Items.Add(option, option.IsProfane);
            }
            audioListBox.EndUpdate();

        }

        public void loadDetectedSexualFromJson(string detectedSexualFrameTimesJson)
        {
            List<FrameInfo> detectedSexualFrameTimes = JsonConvert.DeserializeObject<List<FrameInfo>>(detectedSexualFrameTimesJson);
            foreach (var t in detectedSexualFrameTimes) 
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(t.Milliseconds);
                string durationString = duration.ToString(@"hh\:mm\:ss");

                sexualOptions.Add(new SexualOption
                {
                    DurationFormat = durationString,
                    StartTime = t.Seconds,
                    EndTime = t.NextSeconds
                });
            }

            videoListBox.BeginUpdate();
            foreach (var option in sexualOptions)
            {
                videoListBox.Items.Add(option, true);
            }
            videoListBox.EndUpdate();
        }

        private string censorVideo(string videoFile, Dictionary<string, List<List<double>>> segments, string outputFile) 
        {
            using (Py.GIL())
            {
                dynamic censor = Py.Import("censor");
                dynamic result = censor.censor(videoFile, segments, AppConstants.CENSORED_VIDEO_OUTPUT_FOLER);
                return result.ToString();
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
            List<FrameInfo> sexualFrames = new List<FrameInfo>();

            foreach (var frameInfo in frameInfoList)
            {
                var image = File.ReadAllBytes(frameInfo.FilePath);
                var input = new ModelInput() { ImageSource = image };
                var result = VProfanityModel.Predict(input);
                if (result.PredictedLabel != "safe")
                {
                    sexualFrames.Add(new FrameInfo
                    {
                        Milliseconds = frameInfo.Milliseconds,
                        Seconds = frameInfo.Seconds,
                        NextSeconds = frameInfo.NextSeconds
                    });
                }
            }
            if (sexualFrames.Count > 0)
            {
                AppDBContext appDBContext = new AppDBContext();
                string sexualFrameTimesJson = JsonConvert.SerializeObject(sexualFrames);
                appDBContext.SaveDetectedSexualTimes(videoHash, sexualFrameTimesJson);
            }

        }

        private List<object> FindProjectWordsFromTranscript(List<TranscriptChunk> transcript)
        {
            return null;
        }
         


        private void scanAudio(string videoHash) 
        {
            using (Py.GIL())
            {
                dynamic speechtotext = Py.Import("speechtotext");
                dynamic speech_to_text_result_json = speechtotext.speech_to_text(axWindowsMediaPlayer1.URL);
                string speech_to_text_result_string = speech_to_text_result_json.ToString();

                List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(speech_to_text_result_string);
                markTranscriptProfanity(transcript);
                if (transcript.Count > 0)
                {
                    var appDBContext = new AppDBContext();
                    appDBContext.SaveTranscript(videoHash, JsonConvert.SerializeObject(transcript));
                }

            }

        }

        private void markTranscriptProfanity(List<TranscriptChunk> transcript)
        {
            AppDBContext appDBContext = new AppDBContext();
            foreach (var t in transcript)
            {
                string[] words = t.text.Split(' ');

                foreach (var word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word) && appDBContext.IsProfane(word))
                    {
                        t.hasProfane = true;
                        break;
                    }
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
                loadWordsFromTranscriptJson(transcriptJson);

            }

            string detectedSexualTimesJson = appDBContext.GetDetectedSexualTimes(videoHash);
            if (detectedSexualTimesJson != null)
            {
                loadDetectedSexualFromJson(detectedSexualTimesJson);
            }
            
        }
        private async void scanButton_Click(object sender, EventArgs e)
        {
            if (currentFileHash == null)
            {
                MessageBox.Show("Please upload a video first before scanning.", "No Video Uploaded");
                return;
            }

            AppDBContext appDBContext = new AppDBContext();

            if (appDBContext.HasRecord(currentFileHash))
            {
                DialogResult result = MessageBox.Show("There was a scan performed before, do you want to reuse its data?", "Old Scan Result Found", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    _setDefaultControlState();
                    loadFromDb(currentFileHash);
                    return;
                }
            }

            _setDefaultControlState();

            uploadButton.Enabled = false;
            scanButton.Text = "Scanning";
            scanButton.Enabled = false;
            censorButton.Enabled = false;
            exportButton.Enabled = false;
            var task1 =  Task.Run(() => scanVideo(currentFileHash));
            var task2 =  Task.Run(() => scanAudio(currentFileHash));
            await Task.WhenAll(task1, task2);

            loadFromDb(currentFileHash);

            uploadButton.Enabled = true;
            scanButton.Text = "Scan";
            scanButton.Enabled = true;
            censorButton.Enabled = true;
            exportButton.Enabled = true;

            int profaneCount = wordsOptions.Count(d => d.IsProfane);
            int sexualCount = sexualOptions.Count;
            MessageBox.Show($"The scan has finished.\n\nScan results:\nFound sexual frames: {sexualCount}\nFound profane regions: {profaneCount}", "Scan complete");

        }

        private async void analyzeButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(axWindowsMediaPlayer1.URL))
            {
                MessageBox.Show("Please upload a video first before analyzing the transcript.", "No Video Uploaded");
                return;
            }
            AppDBContext dbContext = new AppDBContext();

            string rawTranscript = dbContext.GetTranscript(currentFileHash);
            if (rawTranscript == null)
            {
                MessageBox.Show("Please scan the video to generate transcript.\nIf there is still no transcript after scanning maybe the video does not contain speech", "No Transcript Found");
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
            audioListBox.BeginUpdate();
            audioListBox.Items.Clear();
            if (!string.IsNullOrWhiteSpace(searchBox.Text))
            {
                WordOption[] searchResult = wordsOptions.Where(w => w.Word.ToLower().Contains(searchBox.Text.ToLower())).ToArray();
                foreach (var result in searchResult)
                {
                    audioListBox.Items.Add(result, result.IsProfane);
                }
            }
            else
            {
                audioListBox.Items.AddRange(wordsOptions.ToArray());
            }

            audioListBox.EndUpdate();
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            if (audioListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a word from the Audio list before exporting.", "No Word Selected");
                return;
            }
            using (ExtractForm extractForm = new ExtractForm())
            {
                extractForm.SelectedItem = (WordOption)audioListBox.SelectedItem;
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
                WordOption[] searchResult = wordsOptions.Where(w => w.IsProfane).ToArray();
                foreach (var result in searchResult)
                {
                    audioListBox.Items.Add(result, true);
                }
            }
            else
            {
                audioListBox.Items.AddRange(wordsOptions.ToArray());
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


        private async void censorButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(axWindowsMediaPlayer1.URL))
            {
                MessageBox.Show("Please upload a video first before scanning.", "No Video Uploaded");
                return;
            }

            Dictionary<string, List<List<double>>> segments = getSegmentsFromListBox();
            if (segments == null)
            {
                MessageBox.Show("Please check at least one item from the video or audio lists", "No Items Selected");
                return;
            }

            uploadButton.Enabled = false;
            scanButton.Enabled = false;
            censorButton.Enabled = false;
            censorButton.Text = "Censoring";
            exportButton.Enabled = false;

            string censoredVideoPath = await Task.Run(() => censorVideo(axWindowsMediaPlayer1.URL, segments, AppConstants.CENSORED_VIDEO_OUTPUT_FOLER));

            uploadButton.Enabled = true;
            scanButton.Enabled = true;
            censorButton.Enabled = true;
            censorButton.Text = "Censor";
            exportButton.Enabled = true;
            MessageBox.Show($"The censored file is saved at {censoredVideoPath}.", "Video Censored Successfully");
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            PythonEngine.Shutdown();

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


    public class SexualOption
    {
        public string DurationFormat { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }


        public override string ToString()
        {
            return DurationFormat;
        }
    }


    public class TranscriptChunk
    {
        public double start { get; set; }
        public double end { get; set; }
        public string text { get; set; }
        public bool hasProfane { get; set; }
    }


    public class FrameInfo
    {
        public string FilePath { get; set; }
        public double Milliseconds { get; set; }
        public double Seconds { get; set; }
        public double NextSeconds { get; set; }
    }


}
