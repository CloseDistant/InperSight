using SqlSugar;
using System;

namespace InperSight.Lib.Bean.Data.Model
{
    [SugarTable("Note", "笔记", IsDisabledUpdateAll = true)]
    public class Note
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        [SugarColumn(ColumnDataType = "text")]
        public string Text { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
