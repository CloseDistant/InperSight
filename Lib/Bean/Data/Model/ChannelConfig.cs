using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Bean.Data.Model
{
    [SugarTable("ChannelConfig", "所有通道基本配置", IsDisabledUpdateAll = true)]
    public class ChannelConfig
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public string ChannelName { get; set; } 
        public string Color { get; set; }
        public string Type { get; set; }
        public double Diameter { get; set; }
        public double RectWidth { get; set; }
        public double RectHeight { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
