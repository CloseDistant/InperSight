using OpenCvSharp;
using System;

namespace InperStudio.Lib.Bean
{
    public class VideoFrame
    {
        public VideoFrame(DateTime time, Mat frameMat)
        {
            Time = time;
            FrameMat = frameMat;
        }

        public DateTime Time { get; set; }

        public Mat FrameMat { get; set; }
    }
}
