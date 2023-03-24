using SqlSugar;
using System;

namespace InperStudio.Lib.Data.Monitor
{
    [SugarTable("inpersignal_module_use", "设备模块使用监控", IsDisabledUpdateAll = true)]
    public class Inpersignal_module_use
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Snumber { get; set; }
        public int Analog_count { get; set; }
        public int Marker_count { get; set; }
        public int Output_count { get; set; }
        public int Stimulus_count { get; set; }
        public int Trigger_count { get; set; }
        public int Note_count { get; set; }
        public int Video_count { get; set; }
        public DateTime? Updatetime { get; set; }
        public DateTime? Createtime { get; set; }
    }
}
