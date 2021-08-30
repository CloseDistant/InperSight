using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("Config", "配置文件", IsDisabledUpdateAll = true)]
    public class Config
    {
        public string PropertyName { get; set; }
        [SugarColumn(ColumnDataType = "text")]
        public string JsonText { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
