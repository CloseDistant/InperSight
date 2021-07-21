using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("Output", "输出的信号", IsDisabledUpdateAll = true)]
    public class Output
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public long CameraTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
