using SqlSugar;
using System;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("ChannelConfig", "通道配置表", IsDisabledUpdateAll = true)]
    public class ChannelConfig
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public int LightId { get; set; }
        public string Color { get; set; }
        public string ChannelName { get; set; }
        public string LightName { get; set; }
        public string Type { get; set; }
        public bool IsDelete { get; set; } = false;
        public DateTime CreateTime { get; set; }
    }
}
