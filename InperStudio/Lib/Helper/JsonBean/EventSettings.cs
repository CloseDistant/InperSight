using InperStudio.Lib.Bean.Channel;
using System.Collections.Generic;
using System.Windows.Media;

namespace InperStudio.Lib.Helper.JsonBean
{
    public class EventSettings
    {
        public List<EventChannelJson> Channels { get; set; }
        public object Marker { get; set; }
        public object Output { get; set; }
    }
    public class EventChannelJson
    {
        public int ChannelId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string BgColor { get; set; } = Brushes.Red.ToString();
        public string Hotkeys { get; set; }
        public double DeltaF { get; set; }
        /// <summary>
        /// 类型  区分是mark通道还是output通道
        /// </summary>
        public string Type { get; set; }
    }
}
