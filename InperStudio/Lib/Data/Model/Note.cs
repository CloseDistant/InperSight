using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("Note", "笔记", IsDisabledUpdateAll = true)]
    public class Note
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        [SugarColumn(ColumnDataType ="text")]
        public string Text { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
