using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("Input", "输入的信号", IsDisabledUpdateAll = true)]
    public class Input
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public double Value { get; set; }
        public long CameraTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
