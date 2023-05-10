using System;
using System.Windows.Forms;

namespace vProfanity
{
    public partial class LoadingForm : Form
    {
        public string LoadingMessage { get; set; }
        public LoadingForm()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private void LoadingForm_Load(object sender, EventArgs e)
        {
            loadingLabel.Text = LoadingMessage;
        }
    }
}
