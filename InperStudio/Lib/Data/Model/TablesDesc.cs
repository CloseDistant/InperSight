using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("TablesDesc", "表阐述", IsDisabledUpdateAll = true)]
    public class TablesDesc
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        [SugarColumn(IsNullable = false)]
        public string TableName { get; set; }
        [SugarColumn(IsNullable = false)]
        public int TableType { get; set; }
        [SugarColumn(ColumnDataType = "text")]
        public string Desc { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
