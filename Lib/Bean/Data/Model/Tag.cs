using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Bean.Data.Model
{
    [SugarTable("Tag", "标签", IsDisabledUpdateAll = true)]
    public class Tag
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public int Type { get; set; }
        public string Value { get; set; }
        public DateTime DateTime { get; set; }
    }
}
