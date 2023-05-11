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
                double fps = Math.Floor(capture.Get(CapProp.Fps));
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
                double fps = Math.Floor(capture.Get(CapProp.Fps));
                Mat frame = new Mat();
                while (capture.Read(frame))
                {
                    
                    double posFrame = capture.Get(CapProp.PosFrames);
                    double milliseconds = capture.Get(CapProp.PosMsec);

                    if (posFrame % fps == 0)
                    {
                        yield return new TimedFrame
                        {
                            Frame = frame,
                            Milliseconds = milliseconds
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
        public double Milliseconds { get; set; }
    }





}
