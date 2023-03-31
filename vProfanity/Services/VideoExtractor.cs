using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace vProfanity.Services
{
    public class VideoExtractor
    {
        public VideoExtractor() 
        {
            FFmpeg.SetExecutablesPath(AppConstants.FFMPEG_EXE);
        }

        public async Task<string> Split(string inputFile, string outputFile, TimeSpan start, TimeSpan end)
        {
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputFile);
            IVideoStream videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            IAudioStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            TimeSpan duration = end.Subtract(start);
            await FFmpeg.Conversions.New()
                .AddStream(videoStream.Split(start, duration))
                .AddStream(audioStream.Split(start, duration))
                .SetOutput(outputFile)
                .SetOutputFormat(Path.GetExtension(outputFile).Substring(1))
                .Start();
            return outputFile;
        }

    }
    
   
}
