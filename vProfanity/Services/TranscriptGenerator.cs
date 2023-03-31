using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace vProfanity.Services
{
    public class TranscriptGenerator
    {
        public TranscriptGenerator() 
        {

        }

        public readonly string BASE_ARGS = "-i <input> -S en -F json -o <output>";
        public readonly string TRANSCRIPT_TEMP_PATH = Path.Combine(Path.GetTempPath(), "WordSearcher");

        public Process GenerateTranscriptProcess(string filePath)
        {

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string args = BASE_ARGS.Replace("<input>", filePath).Replace("<output>", Path.Combine(TRANSCRIPT_TEMP_PATH, $"{fileName}.json"));
            var process = new Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    FileName = Path.Combine(AppConstants.AUTOSUB_FOLDER, "autosub"),
                    Arguments = args,
                    UseShellExecute = true,
                    
                }
            };
            return process;
            
        }

        
    }

    
}
