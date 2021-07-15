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
        public double Brightness { get; set; }
        public RecordMode RecordMode { get; set; }
        public List<WaveGroup> LightMode { get; set; }
        public List<Channel> CameraChannels { get; set; }
    }
    public class RecordMode
    {
        public bool IsContinuous { get; set; }
        public bool IsInterval { get; set; }
        public double Duration { get; set; }
        public double Interval { get; set; }
        public double Delay { get; set; }
    }
    public class Channel
    {
        public int ChannelId { get; set; }
        public string Name { get; set; }
        public double ROI { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double YTop { get; set; }
        public double YBottom { get; set; }
    }
}
