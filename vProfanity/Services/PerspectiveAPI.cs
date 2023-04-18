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
            ScoreResponse averageScoreResponse = new ScoreResponse()
            {
                attributeScores = new AttirubteScores()
                {
                    TOXICITY = new Attribute()
                    {
                        summaryScore = new SummaryScore()
                    },
                    IDENTITY_ATTACK = new Attribute()
                    {
                        summaryScore = new SummaryScore()
                    },
                    INSULT = new Attribute() 
                    { 
                        summaryScore = new SummaryScore() 
                    },
                    PROFANITY = new Attribute() 
                    { 
                        summaryScore = new SummaryScore() 
                    },
                    THREAT = new Attribute() 
                    { 
                        summaryScore = new SummaryScore() 
                    }
                }
            };
            List<ScoreResponse> scoreResponses = new List<ScoreResponse>();
            List<string> textSegments = SplitText(text, 18000);
            foreach (var segment in textSegments)
            {
                string rawJson = JsonConvert.SerializeObject(new
                {
                    comment = new
                    {
                        text = segment
                    },
                    languages = new string[] { "en" },
                    requestedAttributes = new
                    {
                        TOXICITY = new { },
                        IDENTITY_ATTACK = new { },
                        INSULT = new { },
                        PROFANITY = new { },
                        THREAT = new { }
                    }
                });
                var jsonContent = new StringContent(rawJson, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await _httpClient.PostAsync("", jsonContent);
                string json = await response.Content.ReadAsStringAsync();
                ScoreResponse scoreResponse = JsonConvert.DeserializeObject<ScoreResponse>(json);
                scoreResponses.Add(scoreResponse);
            }
            averageScoreResponse.attributeScores.TOXICITY.summaryScore.value = scoreResponses.Select(sr => sr.attributeScores.TOXICITY.summaryScore.value).Average();
            averageScoreResponse.attributeScores.IDENTITY_ATTACK.summaryScore.value = scoreResponses.Select(sr => sr.attributeScores.IDENTITY_ATTACK.summaryScore.value).Average();
            averageScoreResponse.attributeScores.INSULT.summaryScore.value = scoreResponses.Select(sr => sr.attributeScores.INSULT.summaryScore.value).Average();
            averageScoreResponse.attributeScores.PROFANITY.summaryScore.value = scoreResponses.Select(sr => sr.attributeScores.PROFANITY.summaryScore.value).Average();
            averageScoreResponse.attributeScores.THREAT.summaryScore.value = scoreResponses.Select(sr => sr.attributeScores.THREAT.summaryScore.value).Average();


            return averageScoreResponse;


        }

        public List<string> SplitText(string text, int chunkSize)
        {
            var chunks = new List<string>();

            var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var currentChunk = new StringBuilder();
            foreach (var word in words)
            {
                if (currentChunk.Length + word.Length + 1 > chunkSize)
                {
                    chunks.Add(currentChunk.ToString().Trim());
                    currentChunk.Clear();
                }

                currentChunk.Append(word + " ");
            }

            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }

            return chunks;
        }


    }

    public class SummaryScore
    {
        public double value { get; set; }
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
