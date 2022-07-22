using SqlSugar;
using System;

namespace InperStudio.Lib.Data.Model
{
    public class BaseMarker
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        [SugarColumn(IsIgnore = true)]
        public bool IsIgnore { get; set; }//false output
        [SugarColumn(IsIgnore = true)]
        public double RefractoryPeriod { get; set; } = 0;
        public int ChannelId { get; set; }
        public int ConditionId { get; set; }
        public string Name { get; set; }
        public double DeltaF { get; set; }
        public string Color { get; set; }
        public string Type { get; set; }
        public long CameraTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
