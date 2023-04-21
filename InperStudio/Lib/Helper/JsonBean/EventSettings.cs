using InperStudio.Lib.Bean.Channel;
using System.Collections.Generic;
using System.Windows.Media;

namespace InperStudio.Lib.Helper.JsonBean
{
    public class EventSettings
    {
        public List<EventChannelJson> Channels { get; set; }
        public object Marker { get; set; }
        public List<EventChannelJson> Output { get; set; }
    }
    public class EventChannelJson
    {
        public int ChannelId { get; set; }
        public string SymbolName { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public bool IsRefractoryPeriod { get; set; } = false;
        public double RefractoryPeriod { get; set; } = 3;
        public string BgColor { get; set; } = Brushes.Red.ToString();
        public int HotkeysCount { get; set; } = 0;
        private string hotkeys;
        public string Hotkeys
        {
            get { return hotkeys; }
            set
            {
                hotkeys = value;
                if (!string.IsNullOrEmpty(hotkeys))
                {
                    HotkeysCount = hotkeys.Split('+').Length;
                }
            }
        }
        public double DeltaF { get; set; } = 5;
        public int LightIndex { get; set; }
        public int WindowSize { get; set; } = 300;
        public double Tau1 { get; set; }
        public double Tau2 { get; set; }
        public double Tau3 { get; set; }
        public EventChannelJson Condition { get; set; }
        public VideoZone VideoZone { get; set; }
        /// <summary>
        /// 通道类型
        /// </summary>
        public string Type { get; set; }

    }
}
