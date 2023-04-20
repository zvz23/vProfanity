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
using Google.Protobuf.WellKnownTypes;
using System.ComponentModel;
using System.CodeDom;
using Xabe.FFmpeg;

namespace vProfanity
{
    public partial class Main : Form
    {

        private readonly List<WordOption> wordsOptions = new List<WordOption>();
        private readonly List<FrameOption> frameOptions = new List<FrameOption>();
        private string currentFileHash;
        private readonly CheckedListBox audioListBox;
        private readonly CheckedListBox videoListBox;
        public Main()
        {
            InitializeComponent();
            filterComboBox.Items.Add("None");
            filterComboBox.Items.Add("Profane");

            audioListBox = new CheckedListBox()
            {
                Width = tabControl1.Width,
                Height = tabControl1.Height,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray,
            };

            videoListBox = new CheckedListBox()
            {
                Width = tabControl1.Width,
                Height = tabControl1.Height,
                BorderStyle = BorderStyle.None,
                Dock = DockStyle.Fill,
                BackColor = Color.LightGray
            };
            videoListBox.SelectedIndexChanged += videoListBox_SelectedIndexChanged;
            audioListBox.SelectedIndexChanged += audioListBox_SelectedIndexChanged;
            audioListBox.ItemCheck += audioListBox_ItemCheck;
            tabControl1.TabPages[0].Text = "Audio";
            tabControl1.TabPages[0].Controls.Add(audioListBox);
            tabControl1.TabPages[1].Text = "Video";
            tabControl1.TabPages[1].Controls.Add(videoListBox);

        }

        private void audioListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            audioListBox.ItemCheck -= audioListBox_ItemCheck;
            WordOption selectedWord = (WordOption)audioListBox.SelectedItem;
            
            for (int i = 0; i < audioListBox.Items.Count; i++)
            {
                if (!ReferenceEquals(audioListBox.SelectedItem, audioListBox.Items[i]))
                {
                    WordOption tempWordOption = (WordOption)audioListBox.Items[i];
                    if (tempWordOption.StartTime == selectedWord.StartTime && tempWordOption.EndTime == tempWordOption.EndTime)
                    {
                        audioListBox.SetItemChecked(i, e.NewValue == CheckState.Checked);
                    }
                }

            }
            
            audioListBox.ItemCheck += audioListBox_ItemCheck;


        }

        private Dictionary<string, List<List<double>>> getSegmentsFromListBox()
        {
            Dictionary<string, List<List<double>>> segmentsContainer = new Dictionary<string, List<List<double>>>();
            segmentsContainer["video"] = new List<List<double>>();
            segmentsContainer["audio"] = new List<List<double>>();

            if (videoListBox.CheckedItems.Count > 0)
            {
                HashSet<Segment> tempVideoSegments = new HashSet<Segment>();

                foreach (var option in videoListBox.CheckedItems)
                {
                    FrameOption frameOption = (FrameOption)option;
                    tempVideoSegments.Add(new Segment { Start = frameOption.StartTime, End = frameOption.EndTime });
                }
                foreach (var segment in tempVideoSegments)
                {
                    segmentsContainer["video"].Add(new List<double> { segment.Start, segment.End });
                }
            }

            if (audioListBox.CheckedItems.Count > 0)
            {
                HashSet<Segment> tempAudioSegments = new HashSet<Segment>();

                foreach (var option in audioListBox.CheckedItems)
                {
                    WordOption wordOption = (WordOption)option;
                    tempAudioSegments.Add(new Segment { Start = wordOption.StartTime, End = wordOption.EndTime });
                }
                foreach (var segment in tempAudioSegments)
                {
                    segmentsContainer["audio"].Add(new List<double> { segment.Start, segment.End });
                }

            }

            if (segmentsContainer["video"].Count == 0 && segmentsContainer["audio"].Count == 0)
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

            frameOptions.Clear();
            wordsOptions.Clear();

            uploadButton.Enabled = true;
            scanButton.Enabled = true;
            censorButton.Enabled = true;
            extractButton.Enabled = true;
            analyzeButton.Enabled = true;

            uploadButton.Text = "Upload";
            scanButton.Text = "Scan";
            censorButton.Text = "Censor";
            extractButton.Text = "Export";
            analyzeButton.Text = "Analyze Transcript";

        }


        private void uploadButton_Click(object sender, EventArgs e)
        {
            uploadButton.Invoke((MethodInvoker)delegate
            {
                uploadButton.Enabled = false;
                uploadButton.Text = "Uploading";
            });
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
            uploadButton.Invoke((MethodInvoker)delegate
            {
                uploadButton.Enabled = true;
                uploadButton.Text = "Upload";
            });
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
            audioListBox.ItemCheck -= audioListBox_ItemCheck;
            audioListBox.BeginUpdate();
            foreach (var option in wordsOptions)
            {
                audioListBox.Items.Add(option, option.IsProfane);
            }
            audioListBox.EndUpdate();
            audioListBox.ItemCheck += audioListBox_ItemCheck;

        }

        public void loadFramesInfosFromJson(string framesInfosJson)
        {
            List<FrameInfo> framesInfos = JsonConvert.DeserializeObject<List<FrameInfo>>(framesInfosJson);
            videoListBox.BeginUpdate();
            foreach (var t in framesInfos) 
            {
                TimeSpan duration = TimeSpan.FromMilliseconds(t.Milliseconds);
                string durationString = duration.ToString(@"hh\:mm\:ss\.fff");

                FrameOption frameOption = new FrameOption()
                {
                    DurationFormat = durationString,
                    StartTime = t.Seconds,
                    EndTime = t.NextSeconds,
                    IsSexual = t.IsSexual
                };
                frameOptions.Add(frameOption);
                videoListBox.Items.Add(frameOption, frameOption.IsSexual);

            }
            videoListBox.EndUpdate();

        }

        private string censorVideo(string videoFile, Dictionary<string, List<List<double>>> segments, string outputFile) 
        {
            using (Py.GIL())
            {
                dynamic censor = Py.Import("censor");
                dynamic result = censor.censor(videoFile, segments, outputFile);
                return result.ToString();
            }

        }

        public Image ResizeImage(Image image, int desiredHeight)
        {
            int originalWidth = image.Width;
            int originalHeight = image.Height;

            int newWidth = (int)((float)desiredHeight / originalHeight * originalWidth);

            Bitmap resizedImage = new Bitmap(newWidth, desiredHeight);

            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                graphics.DrawImage(image, new Rectangle(0, 0, newWidth, desiredHeight));
            }

            return resizedImage;
        }

        private void scanVideo(string videoHash)
        {
            List<FrameInfo> frameInfoList = null;
            using (Py.GIL())
            {
                string videoFramesPath = Path.Combine(AppConstants.ABS_TEMP_FOLDER, videoHash);
                dynamic image_exporter = Py.Import("image_exporter");
                dynamic frames_info_json = image_exporter.export_video_images_by_keyframes(axWindowsMediaPlayer1.URL, videoFramesPath);
                frameInfoList = JsonConvert.DeserializeObject<List<FrameInfo>>(frames_info_json.ToString());

            }
            if (frameInfoList == null || frameInfoList.Count == 0)
            {
                return;
            }
            foreach (var frameInfo in frameInfoList)
            {
                byte[] imageBytes;
                using (Image image = Image.FromFile(frameInfo.FilePath))
                {
                    using (Image resizedImage = ResizeImage(image, 320))
                    {
                        using (MemoryStream stream = new MemoryStream())
                        {
                            resizedImage.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            imageBytes = stream.ToArray();
                        }
                    }
                }
                
                var input = new ModelInput() { ImageSource = imageBytes };
                var result = VProfanityModel.Predict(input);
                frameInfo.IsSexual = result.PredictedLabel != "safe";
                frameInfo.FilePath = null;
            }
            AppDBContext appDBContext = new AppDBContext();
            string framesInfosJson = JsonConvert.SerializeObject(frameInfoList);
            appDBContext.SaveFramesInfos(videoHash, framesInfosJson);

        }


        private void scanAudio(string videoHash) 
        {
            string tempDirPath = Path.Combine(AppConstants.ABS_TEMP_FOLDER, videoHash);
            using (Py.GIL())
            {
                dynamic speechtotext = Py.Import("speechtotext");
                dynamic speech_to_text_result_json = speechtotext.speech_to_text(axWindowsMediaPlayer1.URL, tempDirPath);
                if(speech_to_text_result_json == null)
                {
                    return;
                }
                string speech_to_text_result_string = speech_to_text_result_json.ToString();
                List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(speech_to_text_result_string);
                markTranscriptProfanity(transcript);
                var appDBContext = new AppDBContext();
                appDBContext.SaveTranscript(videoHash, JsonConvert.SerializeObject(transcript));

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
                loadFramesInfosFromJson(detectedSexualTimesJson);
            }
            
        }

        private void deleteTempData(string videoHash)
        {
            string tempDir = Path.Combine(AppConstants.ABS_TEMP_FOLDER, videoHash);
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
        private async void scanButton_Click(object sender, EventArgs e)
        {
            if (currentFileHash == null)
            {
                MessageBox.Show("Please upload a video first before scanning.", "No Video Uploaded", MessageBoxButtons.OK);
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

            uploadButton.Invoke((MethodInvoker)delegate
            {
                uploadButton.Enabled = false;
            });
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Text = "Scanning";
                scanButton.Enabled = false;
            });
            censorButton.Invoke((MethodInvoker)delegate
            {
                censorButton.Enabled = false;
            });

            extractButton.Invoke((MethodInvoker)delegate
            {
                extractButton.Enabled = false;
            });

            analyzeButton.Invoke((MethodInvoker)delegate
            {
                analyzeButton.Enabled = false;
            });



            var task1 =  Task.Run(() => scanVideo(currentFileHash));
            var task2 =  Task.Run(() => scanAudio(currentFileHash));
            await Task.WhenAll(task1, task2);

            await Task.Run(() => deleteTempData(currentFileHash));
            loadFromDb(currentFileHash);
            uploadButton.Invoke((MethodInvoker)delegate
            {
                uploadButton.Enabled = true;
            });
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Text = "Scan";
                scanButton.Enabled = true;
            });
            censorButton.Invoke((MethodInvoker)delegate
            {
                censorButton.Enabled = true;
            });

            extractButton.Invoke((MethodInvoker)delegate
            {
                extractButton.Enabled = true;
            });

            analyzeButton.Invoke((MethodInvoker)delegate
            {
                analyzeButton.Enabled = true;
            });



            int profaneCount = wordsOptions.Count(d => d.IsProfane);
            int sexualCount = frameOptions.Count(d => d.IsSexual);
            MessageBox.Show($"The scan has finished.\n\nScan results:\nFound sexual frames: {sexualCount}\nFound profane regions: {profaneCount}", "Scan complete", MessageBoxButtons.OK);

        }

        private async void analyzeButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(axWindowsMediaPlayer1.URL))
            {
                MessageBox.Show("Please upload a video first before analyzing the transcript.", "No Video Uploaded", MessageBoxButtons.OK);
                return;
            }
            AppDBContext dbContext = new AppDBContext();

            string rawTranscript = dbContext.GetTranscript(currentFileHash);
            if (rawTranscript == null)
            {
                MessageBox.Show("Please scan the video to generate transcript.\nIf there is still no transcript after scanning maybe the video does not contain speech", "No Transcript Found", MessageBoxButtons.OK);
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
            using (TranscriptAnalyzeResultForm form = new TranscriptAnalyzeResultForm())
            {
                form.Transcript = stringBuilder;
                form.ShowDialog(this);
            }



            analyzeButton.Text = "Analyze Transcript";
            analyzeButton.Enabled = true;
        }

        private void searchTextBox_TextChanged(object sender, EventArgs e)
        {
            audioListBox.ItemCheck -= audioListBox_ItemCheck;
            audioListBox.BeginUpdate();
            audioListBox.Items.Clear();
            if (!string.IsNullOrWhiteSpace(searchTextBox.Text))
            {
                WordOption[] searchResult = wordsOptions.Where(w => w.Word.ToLower().Contains(searchTextBox.Text.ToLower())).ToArray();
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
            audioListBox.ItemCheck += audioListBox_ItemCheck;

        }

        private void extractButton_Click(object sender, EventArgs e)
        {
            if (audioListBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a word from the Audio list before exporting.", "No Word Selected", MessageBoxButtons.OK);
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
            if (tabControl1.SelectedTab.Text == "Audio")
            {
                audioListBox.ItemCheck -= audioListBox_ItemCheck;
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
                    foreach (var option in wordsOptions)
                    {
                        audioListBox.Items.Add(option, option.IsProfane);
                    }
                }
                audioListBox.EndUpdate();
                audioListBox.ItemCheck += audioListBox_ItemCheck;
            }

            else
            {
                videoListBox.BeginUpdate();
                videoListBox.Items.Clear();
                if (filterComboBox.Text == "Sexual")
                {
                    FrameOption[] searchResult = frameOptions.Where(f => f.IsSexual).ToArray();
                    foreach (var result in searchResult)
                    {
                        videoListBox.Items.Add(result, true);
                    }
                }
                else
                {
                    foreach (var option in frameOptions)
                    {
                        videoListBox.Items.Add(option, option.IsSexual);
                    }
                }
                videoListBox.EndUpdate();

            }
            




        }

        private void videoListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (videoListBox.SelectedItem != null)
            {
                FrameOption sexualOption = (FrameOption)videoListBox.SelectedItem;
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



        private void addRegionButton_Click(object sender, EventArgs e)
        {
            if (currentFileHash == null)
            {
                MessageBox.Show("Please upload a video first before adding region.", "No Video Uploaded", MessageBoxButtons.OK);
                return;
            }
            using (AddVideoRegionForm form = new AddVideoRegionForm())
            {
                form.VideodDuration = axWindowsMediaPlayer1.currentMedia.duration;
                DialogResult result = form.ShowDialog(this);
                if (result != DialogResult.OK)
                {
                    return;
                }

                if (form.SelectedAddRadio == AddVideoRegionForm.SelectedRadio.SECOND)
                {
                    addSecondToVideoListBox(form.SecondsEntered);
                }
                else if (form.SelectedAddRadio == AddVideoRegionForm.SelectedRadio.RANGE)
                {
                    addRangeToVideoListBox(form.StartRangeEntered, form.EndRangeEntered);
                }


            }
        }

        private void addRangeToVideoListBox(double startRange, double endRange)
        {
            TimeSpan startRangeTs = TimeSpan.FromSeconds(startRange);
            TimeSpan endRangeTs = TimeSpan.FromSeconds(endRange);

            int start = Convert.ToInt32(startRangeTs.TotalSeconds);
            int end = Convert.ToInt32(endRangeTs.TotalSeconds);
            videoListBox.BeginUpdate();

            for(int i = start; i <= end; i++)
            {
                videoListBox.Items.Add(new FrameOption
                {
                    DurationFormat = TimeSpan.FromSeconds(Convert.ToDouble(i)).ToString(@"hh\:mm\:ss\.fff"),
                    StartTime = TimeSpan.FromSeconds(i).TotalSeconds,
                    EndTime = TimeSpan.FromSeconds(i).Add(TimeSpan.FromSeconds(1)).TotalSeconds

                }, true);
            }


            videoListBox.EndUpdate();




        }
        private void addSecondToVideoListBox(double second)
        {
            videoListBox.BeginUpdate();
            TimeSpan secondTs = TimeSpan.FromSeconds(second);
            TimeSpan nextSecondTs = secondTs.Add(TimeSpan.FromSeconds(1));
            string secondString = secondTs.ToString(@"hh\:mm\:ss\.fff");
            videoListBox.Items.Add(new FrameOption
            {
                DurationFormat = secondString,
                StartTime = second,
                EndTime = nextSecondTs.Seconds

            }, true);
            videoListBox.EndUpdate();
        }



        private async void censorButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(axWindowsMediaPlayer1.URL))
            {
                MessageBox.Show("Please upload a video first before scanning.", "No Video Uploaded", MessageBoxButtons.OK);
                return;
            }

            Dictionary<string, List<List<double>>> segments = getSegmentsFromListBox();
            if (segments == null)
            {
                MessageBox.Show("Please check at least one item from the video or audio lists", "No Items Selected", MessageBoxButtons.OK);
                return;
            }

            uploadButton.Invoke((MethodInvoker)delegate
            {
                uploadButton.Enabled = false;
            });
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Enabled = false;
            });
            censorButton.Invoke((MethodInvoker)delegate
            {
                censorButton.Enabled = false;
                censorButton.Text = "Censoring";
            });

            extractButton.Invoke((MethodInvoker)delegate
            {
                extractButton.Enabled = false;
            });

            analyzeButton.Invoke((MethodInvoker)delegate
            {
                analyzeButton.Enabled = false;
            });

            string censoredVideoPath = await Task.Run(() => censorVideo(axWindowsMediaPlayer1.URL, segments, AppConstants.CENSORED_VIDEO_OUTPUT_FOLER));

            uploadButton.Invoke((MethodInvoker)delegate
            {
                uploadButton.Enabled = true;
            });
            scanButton.Invoke((MethodInvoker)delegate
            {
                scanButton.Enabled = true;
            });
            censorButton.Invoke((MethodInvoker)delegate
            {
                censorButton.Enabled = true;
                censorButton.Text = "Censor";
            });

            extractButton.Invoke((MethodInvoker)delegate
            {
                extractButton.Enabled = true;
            });

            analyzeButton.Invoke((MethodInvoker)delegate
            {
                analyzeButton.Enabled = true;
            });
            MessageBox.Show($"The censored file is saved at {censoredVideoPath}.", "Video Censored Successfully", MessageBoxButtons.OK);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            PythonEngine.Shutdown();

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            filterComboBox.BeginUpdate();
            filterComboBox.Items.Clear();
            filterComboBox.Items.Add("None");
            if (tabControl1.SelectedTab.Text == "Video")
            {
                filterComboBox.Items.Add("Sexual");
            }
            else
            {
                filterComboBox.Items.Add("Profane");
            }
            filterComboBox.EndUpdate();
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


    public class FrameOption
    {
        public string DurationFormat { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public bool IsSexual { get; set; }


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
        public bool IsSexual { get; set; }
    }


    struct Segment
    {
        public double Start;
        public double End;

        public Segment(double start, double end)
        {
            Start = start;
            End = end;
        }
    }


}
