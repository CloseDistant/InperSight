using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("AIROI", "AI/ROI 类型的marker", IsDisabledUpdateAll = true)]
    public class AIROI
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public int ChannelId { get; set; }
        public double DeltaF { get; set; }
        public string Type { get; set; }
        public long CameraTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
