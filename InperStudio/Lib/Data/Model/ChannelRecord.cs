using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("ChannelRecord", "通道数据记录", IsDisabledUpdateAll = true)]
    public class ChannelRecord
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        //[SugarColumn(IsNullable =false,IndexGroupNameList =new string[] {"" })]
        public int ChannelId { get; set; }
        public double Value { get; set; }
        public int Type { get; set; }
        public long CameraTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
