using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
