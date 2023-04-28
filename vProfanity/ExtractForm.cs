using AxWMPLib;
using vProfanity.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xabe.FFmpeg;

namespace vProfanity
{
    public partial class ExtractForm : Form
    {
        public WordOption SelectedItem { get; set; }
        public string VideoURL { get; set; }
        public ExtractForm()
        {
            InitializeComponent();
        }

        

        private void ExtractForm_Load(object sender, EventArgs e)
        {
            selectedWordLabel.Text = SelectedItem.Word;
            startLabel.Text = SelectedItem.StartTime.ToString();
            endLabel.Text = SelectedItem.EndTime.ToString();
            string[] formats = Enum.GetNames(typeof(Format));
            fileTypeComboBox.Items.AddRange(formats);
        }

        private async void extractButton_Click(object sender, EventArgs e)
        {
            var videoExtractor = new VideoExtractor();

            if (string.IsNullOrWhiteSpace(fileNameTextBox.Text))
            {
                MessageBox.Show("Plese enter the name of the file to extract");
                return;
            }
            string[] formats = Enum.GetNames(typeof(Format));
            if (string.IsNullOrWhiteSpace(fileTypeComboBox.Text) || !formats.Contains(fileTypeComboBox.Text))
            {
                MessageBox.Show("Please select a valid file type");
                return;
            }
            string fileName = $"{fileNameTextBox.Text}.{fileTypeComboBox.Text}";
            string filePath = Path.Combine(AppConstants.CENSORED_VIDEO_OUTPUT_FOLER, fileName);
            if (File.Exists(filePath))
            {
                MessageBox.Show($"The file {fileName} already exists");
                return;
            }

            extractButton.Text = "Extracting";
            extractButton.Enabled = false;
            await videoExtractor.Split(VideoURL, filePath, TimeSpan.FromSeconds(SelectedItem.StartTime), TimeSpan.FromSeconds(SelectedItem.EndTime));
            MessageBox.Show($"The file is saved to {filePath}", "Extraction Successful", MessageBoxButtons.OK);
            this.Close();
        }

        private void fileTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
