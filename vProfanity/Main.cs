using CsharpOpenCVProject;
using Emgu.CV;
using Emgu.CV.Structure;
using Newtonsoft.Json;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using vProfanity.Services;
using static vProfanity.VProfanityModel;

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

            filterComboBox.SelectedIndex = 0;

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
                segmentsContainer["video"].AddRange(tempVideoSegments.Select(s => new List<double>() { s.Start, s.End }).ToList());
            }

            if (audioListBox.CheckedItems.Count > 0)
            {
                HashSet<Segment> tempAudioSegments = new HashSet<Segment>();

                foreach (var option in audioListBox.CheckedItems)
                {
                    WordOption wordOption = (WordOption)option;
                    tempAudioSegments.Add(new Segment { Start = wordOption.StartTime, End = wordOption.EndTime });
                }
                segmentsContainer["audio"].AddRange(tempAudioSegments.Select(s => new List<double>() { s.Start, s.End }).ToList());

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
                TimeSpan duration = TimeSpan.FromSeconds(t.Seconds);
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

        private void scanVideo()
        {
            FramesGenerator framesGenerator = new FramesGenerator(axWindowsMediaPlayer1.URL);
            List<FrameInfo> frames = new List<FrameInfo>();
            FrameInfo prevFrame = null;
            foreach (TimedFrame frame in framesGenerator.GetTimedKeyFrames())
            {
                FrameInfo frameInfo = new FrameInfo();

                frameInfo.Seconds = frame.Milliseconds / 1000.0;
                if (prevFrame != null)
                {
                    prevFrame.NextSeconds = frameInfo.Seconds;
                }
                prevFrame = frameInfo;
                byte[] frameBytes = ProcessFrame(frame.Frame);
                ModelInput inputModel = new ModelInput() { ImageSource = frameBytes };
                frameInfo.IsSexual = VProfanityModel.Predict(inputModel).PredictedLabel != "safe";
                frames.Add(frameInfo);
            }
            prevFrame.NextSeconds = prevFrame.Seconds + 1.0;
            string framesJson = JsonConvert.SerializeObject(frames);

            AppDBContext appDBContext = new AppDBContext();

            appDBContext.SaveFramesInfos(currentFileHash, framesJson);

        }


        private void scanAudio(string wavFile)
        {
            string tempDirPath = Path.Combine(AppConstants.ABS_TEMP_FOLDER, currentFileHash);
            using (Py.GIL())
            {
                dynamic speechtotext = Py.Import("speechtotext");
                dynamic speech_to_text_result_json = speechtotext.speech_to_text(wavFile, tempDirPath);
                if (speech_to_text_result_json == null)
                {
                    return;
                }
                string speech_to_text_result_string = speech_to_text_result_json.ToString();
                List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(speech_to_text_result_string);
                markTranscriptProfanity(transcript);
                var appDBContext = new AppDBContext();
                appDBContext.SaveTranscript(currentFileHash, JsonConvert.SerializeObject(transcript));

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

            string keyFramesJson = appDBContext.GetKeyFrames(videoHash);
            if (keyFramesJson != null)
            {
                loadFramesInfosFromJson(keyFramesJson);
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
            LoadingForm loadingForm = new LoadingForm()
            {
                LoadingMessage = "Scanning for sexual and profane content...Please wait"
            };

            loadingForm.Show(this);
            FFmpegUtils ffmpegUtils = new FFmpegUtils(Path.Combine(AppConstants.ABS_TEMP_FOLDER, currentFileHash));
            string wavFile = await ffmpegUtils.GetWav(axWindowsMediaPlayer1.URL);
            var task1 = Task.Run(() => scanVideo());
            var task2 = Task.Run(() => scanAudio(wavFile));

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

            HashSet<Segment> audioSegments = new HashSet<Segment>();
            wordsOptions.ForEach(w =>
            {
                audioSegments.Add(new Segment
                {
                    Start = w.StartTime,
                    End = w.EndTime
                });
            });
            loadingForm.Close();
            int audioSegmentsCount = audioSegments.Count;
            int keyFramesCount = frameOptions.Count;
            int profaneCount = wordsOptions.Count(d => d.IsProfane);
            int sexualCount = frameOptions.Count(d => d.IsSexual);
            string scanMessage = $"The scan has finished.\n\nScan results:\nFound profane regions: {profaneCount}\nFound sexual keyframes: {sexualCount}";
            MessageBox.Show(this, scanMessage, "Scan complete", MessageBoxButtons.OK);

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
                FrameOption frameOption = (FrameOption)videoListBox.SelectedItem;
                axWindowsMediaPlayer1.Ctlcontrols.currentPosition = frameOption.StartTime;
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

            for (int i = start; i <= end; i++)
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
            LoadingForm loadingForm = new LoadingForm()
            {
                LoadingMessage = "Censoring video... Please wait"
            };
            loadingForm.Show(this);
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
            loadingForm.Close();
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
            filterComboBox.SelectedIndex = 0;
            filterComboBox.EndUpdate();
        }

        private byte[] ProcessFrame(Mat mat)
        {
            int desiredHeight = 320; // Training images height
            double aspectRatio = (double)mat.Width / mat.Height;
            int desiredWidth = (int)(320 * aspectRatio);
            Mat resizedFrame = new Mat();
            CvInvoke.Resize(mat, resizedFrame, new Size(desiredWidth, desiredHeight));
            return resizedFrame.ToImage<Bgr, byte>().ToJpegData();
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
