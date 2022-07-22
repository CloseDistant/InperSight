using SqlSugar;
using System;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("Config", "配置文件", IsDisabledUpdateAll = true)]
    public class Config
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string PropertyName { get; set; }
        [SugarColumn(ColumnDataType = "text")]
        public string JsonText { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
