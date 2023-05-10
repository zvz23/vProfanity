using System;
using System.Windows.Forms;

namespace vProfanity
{
    public partial class AddVideoRegionForm : Form
    {
        public AddVideoRegionForm()
        {
            InitializeComponent();
        }

        public double VideodDuration { get; set; }
        public double SecondsEntered { get; set; }
        public double StartRangeEntered { get; set; }
        public double EndRangeEntered { get; set; }
        public SelectedRadio SelectedAddRadio { get; set; } = SelectedRadio.SECOND;



        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                if (string.IsNullOrWhiteSpace(secondTextBox.Text))
                {
                    MessageBox.Show("Please enter the specific second you want to add.", "No Specific Second Entered", MessageBoxButtons.OK);
                    return;
                }
                double tempSecond;
                if (!double.TryParse(secondTextBox.Text.Trim(), out tempSecond))
                {
                    MessageBox.Show("Please enter a valid second.", "Invalid Second Entered", MessageBoxButtons.OK);
                    return;
                }
                SecondsEntered = tempSecond;

                if (!(SecondsEntered > 0 && SecondsEntered <= VideodDuration))
                {
                    MessageBox.Show("Please enter a valid second of the video.", "Invalid Second Entered", MessageBoxButtons.OK);
                    return;
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else if (radioButton2.Checked)
            {
                if (string.IsNullOrWhiteSpace(startTextBox.Text))
                {
                    MessageBox.Show("Please enter the start range.", "No Start Range Entered", MessageBoxButtons.OK);
                    return;
                }
                if (string.IsNullOrWhiteSpace(endTextBox.Text))
                {
                    MessageBox.Show("Please enter the end range.", "No End Range Entered", MessageBoxButtons.OK);
                    return;
                }

                double tempStart;
                double tempEnd;
                if (!double.TryParse(startTextBox.Text.Trim(), out tempStart))
                {
                    MessageBox.Show("Please enter a valid start range of the video.", "Invalid Start Range Entered", MessageBoxButtons.OK);
                    return;
                }

                if (!double.TryParse(endTextBox.Text.Trim(), out tempEnd))
                {
                    MessageBox.Show("Please enter a valid start range of the video.", "Invalid Start Range Entered", MessageBoxButtons.OK);
                    return;
                }

                if (!(tempStart != tempEnd && tempStart > 0 && tempStart < VideodDuration && tempStart < tempEnd))
                {
                    MessageBox.Show("Please enter a valid start range of the video.", "Invalid Start Range Entered", MessageBoxButtons.OK);
                    return;
                }

                if (!(tempEnd != tempStart && tempEnd > tempStart && tempEnd <= VideodDuration))
                {
                    MessageBox.Show("Please enter a valid end range of the video.", "Invalid End Range Entered", MessageBoxButtons.OK);
                    return;
                }

                StartRangeEntered = tempStart;
                EndRangeEntered = tempEnd;
                this.DialogResult = DialogResult.OK;
                this.Close();

            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            this.SelectedAddRadio = SelectedRadio.SECOND;
            secondCaptionLabel.Visible = true;
            secondLabel.Visible = true;
            secondTextBox.Visible = true;

            rangeCaptionLabel.Visible = false;
            startLabel.Visible = false;
            startTextBox.Visible = false;
            endLabel.Visible = false;
            endTextBox.Visible = false;

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            this.SelectedAddRadio = SelectedRadio.RANGE;

            secondCaptionLabel.Visible = false;
            secondLabel.Visible = false;
            secondTextBox.Visible = false;

            rangeCaptionLabel.Visible = true;
            startLabel.Visible = true;
            startTextBox.Visible = true;
            endLabel.Visible = true;
            endTextBox.Visible = true;
        }

        public enum SelectedRadio
        {
            SECOND,
            RANGE
        }

        private void AddVideoRegionForm_Load(object sender, EventArgs e)
        {
            videoDurationLabel.Text = VideodDuration.ToString();
        }
    }
}
