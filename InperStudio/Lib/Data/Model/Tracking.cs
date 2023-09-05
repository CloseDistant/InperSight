using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("Tracking", "轨迹追踪", IsDisabledUpdateAll = true)]
    public class Tracking
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public float Max_score { get; set; }
        public long CameraTime { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
