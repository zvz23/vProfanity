using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;

namespace vProfanity.Services
{
    public class FFmpegUtils
    {
        public string OutputPath { get; set; }

        public FFmpegUtils(string outputPath)
        {
            FFmpeg.SetExecutablesPath(AppConstants.FFMPEG_EXE);
            OutputPath = outputPath;
        }

        public async Task<string> GetWav(string filePath)
        {
            string outputFileName = $"{Guid.NewGuid()}.wav";
            string outputFilePath = Path.Combine(OutputPath, outputFileName);
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(filePath);
            IAudioStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            await FFmpeg.Conversions.New()
                .AddStream(audioStream)
                .AddParameter("-hide_banner", ParameterPosition.PreInput)
                .AddParameter("-vn -ac 1 -ar 48000", ParameterPosition.PostInput)
                .SetOutput(outputFilePath)
                .Start();
            return outputFilePath;
        }

        public async Task<string> Split(string inputFile, string fileName, TimeSpan start, TimeSpan end)
        {
            string filePath = Path.Combine(OutputPath, fileName);
            IMediaInfo mediaInfo = await FFmpeg.GetMediaInfo(inputFile);
            IVideoStream videoStream = mediaInfo.VideoStreams.FirstOrDefault();
            IAudioStream audioStream = mediaInfo.AudioStreams.FirstOrDefault();
            TimeSpan duration = end.Subtract(start);
            await FFmpeg.Conversions.New()
                .AddStream(videoStream.Split(start, duration))
                .AddStream(audioStream.Split(start, duration))
                .SetOutput(filePath)
                .SetOutputFormat(Path.GetExtension(filePath).Substring(1))
                .Start();
            return filePath;
        }

    }
}
