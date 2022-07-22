using SqlSugar;
using System;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("Environment", "环境", IsDisabledUpdateAll = true)]
    public class Environment
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
