using SqlSugar;
using System;


namespace InperStudio.Lib.Data.Monitor
{
    [SugarTable("device_log", "设备日志状态", IsDisabledUpdateAll = true)]
    public class Device_Log
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Snumber { get; set; }
        public string Error_Message { get; set; }
        public int Error_Level { get; set; }
        public int Is_Dispose { get; set; }
        public int Type { get; set; }
        public DateTime Createtime { get; set; }
    }
}
