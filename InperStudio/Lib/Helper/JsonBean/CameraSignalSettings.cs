using InperStudio.Lib.Bean.Channel;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Helper.JsonBean
{
    public class CameraSignalSettings
    {
        public double Exposure { get; set; }
        public double Sampling { get; set; }
        public double Gain { get; set; }
        public double Brightness { get; set; }
        public RecordMode RecordMode { get; set; }
        public List<WaveGroup> LightMode { get; set; }
        public List<Channel> CameraChannels { get; set; }
    }
    public class RecordMode
    {
        public bool IsContinuous { get; set; }
        public bool IsInterval { get; set; }
        public double Duration { get; set; } = 3;
        public double Interval { get; set; } = 2;
        public double Delay { get; set; } = 1;
    }
    public class Channel
    {
        public int ChannelId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public double ROI { get; set; }
        public double DeltaF { get; set; }
        public double Tau1 { get; set; } = 0.2;
        public double Tau2 { get; set; } = 0.75;
        public double Tau3 { get; set; } = 3;
        public double Left { get; set; }
        public double Top { get; set; }
        public double YTop { get; set; } = 20;
        public double YBottom { get; set; } = 0;
        public bool Offset { get; set; }
        public int OffsetWindowSize { get; set; } = 300;
        public Filters Filters { get; set; } = new Filters();
    }
    public class Filters
    {
        public bool IsLowPass { get; set; }
        public double LowPass { get; set; }
        public bool IsHighPass { get; set; }
        public double HighPass { get; set; }
        public bool IsNotch { get; set; }
        public double Notch { get; set; }
        public bool IsSmooth { get; set; } = true;
        public double Smooth { get; set; } = 5;
    }
}
