using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("IntervalScale", "间隔刻度", IsDisabledUpdateAll = true)]
    public class IntervalScale
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public int Type { get; set; }
        public long CreateTime { get; set; }
    }
}
