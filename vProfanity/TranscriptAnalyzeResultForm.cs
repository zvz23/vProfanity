using System;
using System.Text;
using System.Windows.Forms;
using vProfanity.Services;

namespace vProfanity
{
    public partial class TranscriptAnalyzeResultForm : Form
    {
        public StringBuilder Transcript { get; set; }
        public TranscriptAnalyzeResultForm()
        {
            InitializeComponent();

        }

        private async void TranscriptAnalyzeResultForm_Load(object sender, EventArgs e)
        {
            PerspectiveAPI perspectiveAPI = new PerspectiveAPI();
            ScoreResponse scoreResponse = await perspectiveAPI.AnaylizeText(Transcript.ToString());
            string toxicityStr = scoreResponse.attributeScores.TOXICITY.summaryScore.value.ToString();
            string identityAttackStr = scoreResponse.attributeScores.IDENTITY_ATTACK.summaryScore.value.ToString();
            string insultStr = scoreResponse.attributeScores.INSULT.summaryScore.value.ToString();
            string profanityStr = scoreResponse.attributeScores.PROFANITY.summaryScore.value.ToString();
            string threatStr = scoreResponse.attributeScores.THREAT.summaryScore.value.ToString();

            toxicityValueLabel.Text = $"{toxicityStr[2]} of out 10 people";
            identityAttackValueLabel.Text = $"{identityAttackStr[2]} of out 10 people";
            insultValueLabel.Text = $"{insultStr[2]} of out 10 people";
            profanityValueLabel.Text = $"{profanityStr[2]} of out 10 people";
            threatValueLabel.Text = $"{threatStr[2]} of out 10 people";
        }
    }
}
