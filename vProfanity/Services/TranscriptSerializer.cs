using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace vProfanity.Services
{
    public static class TranscriptSerializer
    {
        public static List<TranscriptChunk> DeserializeFile(string jsonPath)
        {
            string json = File.ReadAllText(jsonPath);
            List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(json);
            return transcript;
        }

        public static List<TranscriptChunk> Deserialize(string json)
        {
            List<TranscriptChunk> transcript = JsonConvert.DeserializeObject<List<TranscriptChunk>>(json);
            return transcript;
        }

    }

    public class TranscriptChunk
    {
        public double start { get; set; }
        public double end { get; set; }
        public string content { get; set; }
    }



}
