using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace vProfanity.Services
{
    public class PerspectiveAPI
    {

        private const string BASE_URL = "https://commentanalyzer.googleapis.com/v1alpha1/comments:analyze?key=AIzaSyCKM5W_RnLQXzOHb_AnIAXX8YzkUx38l4g";
        private readonly HttpClient _httpClient;

        public PerspectiveAPI()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BASE_URL)
            };
        }

        public async Task<ScoreResponse> AnaylizeText(string text)
        {
            string rawJson = JsonConvert.SerializeObject(new
            {
                comment = new
                {
                    text = text
                },
                languages = new string[] { "en" },
                requestedAttributes = new
                {
                    TOXICITY = new {},
                    IDENTITY_ATTACK = new {},
                    INSULT = new {},
                    PROFANITY = new {},
                    THREAT = new {}
                }
            });
            var jsonContent = new StringContent(rawJson,Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("", jsonContent);
            string json = await response.Content.ReadAsStringAsync();
            ScoreResponse scoreResponse = JsonConvert.DeserializeObject<ScoreResponse>(json);
            return scoreResponse;
        }


    }

    public class SummaryScore
    {
        public string value { get; set; }
        public string type { get; set; }
    }

    public class Attribute
    {
        public SummaryScore summaryScore { get; set; }
    }

    public class AttirubteScores
    {
        public Attribute TOXICITY { get; set; }
        public Attribute IDENTITY_ATTACK { get; set; }
        public Attribute INSULT { get; set; }
        public Attribute PROFANITY { get; set; }
        public Attribute THREAT { get; set; }
    }

    public class ScoreResponse
    {
        public AttirubteScores attributeScores { get; set; }
    
    }
}
