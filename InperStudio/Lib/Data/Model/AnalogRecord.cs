using SqlSugar;
using System;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("AnalogRecord", "ad通道数据记录", IsDisabledUpdateAll = true)]
    public class AnalogRecord
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        //[SugarColumn(IsNullable =false,IndexGroupNameList =new string[] {"" })]
        public int ChannelId { get; set; }
        public string Value { get; set; }
        public long CameraTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
