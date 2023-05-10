using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.IO;

namespace CsharpOpenCVProject
{
    public class FramesGenerator
    {
        public readonly string VideoFile;

        public FramesGenerator(string videoFile)
        {
            if (!File.Exists(videoFile))
            {
                throw new FileNotFoundException($"File {videoFile} does not exists.");
            }

            VideoFile = videoFile;
        }

        public IEnumerable<Mat> GetKeyframes()
        {
            using (VideoCapture capture = new VideoCapture(VideoFile))
            {
                double fps = Math.Round(capture.Get(CapProp.Fps));
                Mat frame = new Mat();
                while (capture.Read(frame))
                {
                    double posFrame = capture.Get(CapProp.PosFrames);
                    if (posFrame % fps == 0)
                    {
                        yield return frame;
                    }

                }
            }
        }

        public IEnumerable<TimedFrame> GetTimedKeyFrames()
        {
            using (VideoCapture capture = new VideoCapture(VideoFile))
            {
                double fps = Math.Round(capture.Get(CapProp.Fps));
                Mat frame = new Mat();
                while (capture.Read(frame))
                {

                    double posFrame = capture.Get(CapProp.PosFrames);
                    double frameSeconds = capture.Get(CapProp.PosMsec) / 1000;

                    if (posFrame % fps == 0)
                    {
                        yield return new TimedFrame
                        {
                            Frame = frame,
                            Seconds = frameSeconds
                        };
                    }

                }
            }
        }


        public IEnumerable<Mat> GetFrames()
        {
            using (VideoCapture capture = new VideoCapture(VideoFile))
            {
                Mat frame = new Mat();
                while (capture.Read(frame))
                {
                    yield return frame;
                }
            }
        }
    }

    public class TimedFrame
    {
        public Mat Frame { get; set; }
        public double Seconds { get; set; }
    }





}
