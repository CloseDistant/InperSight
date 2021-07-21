using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("ColumnDesc", "列阐述", IsDisabledUpdateAll = true)]
    public class ColumnDesc
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        [SugarColumn(IsNullable = false)]
        public string TableName { get; set; }
        [SugarColumn(IsNullable = false)]
        public string ColumnName { get; set; }
        public int ColumnType { get; set; }
        [SugarColumn(ColumnDataType = "text")]
        public string Desc { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
