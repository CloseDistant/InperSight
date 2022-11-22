using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Config.Json
{
    public class CameraSettingJsonBean
    {
        public int FPS { get; set; } = 20;
        public double Gain { get; set; } = 1;
        public double FocusPlane { get; set; } = 0;
        public double UpperLevel { get; set; } = 1;
        public double LowerLevel { get; set; } = 0;
        public double ExcitLowerLevel { get; set; } = 10;
        public List<CameraChannelConfig> CameraChannelConfigs { get; set; } = new List<CameraChannelConfig>();
    }

    public class CameraChannelConfig
    {
        public string Name { get; set; }
        public int ChannelId { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public string Color { get; set; }
        public double Diameter { get; set; }
        public double RectWidth { get; set; }
        public double RectHeight { get; set; }
        public string Type { get; set; }
    }
}
