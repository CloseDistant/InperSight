using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Bean.Data.Model
{
    [SugarTable("ChannelRecord", "所有通道数据记录", IsDisabledUpdateAll = true)]
    public class ChannelRecord
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Value { get; set; }
        public int Type { get; set; } = 0;
        public long CameraTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
