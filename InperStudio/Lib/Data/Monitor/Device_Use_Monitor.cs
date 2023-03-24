using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Monitor
{
    [SugarTable("device_use_monitor", "设备使用监控", IsDisabledUpdateAll = true)]
    public class Device_Use_Monitor
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Snumber { get; set; }
        public int Open_Count { get; set; }
        public int Record_Count { get; set; }
        /// <summary>
        /// 打开软件的总时长（记录了数据才会累加 ，只打开软件不记录数据不累加）
        /// </summary>
        public double Total_Open_Time { get; set; }
        public double Total_Record_Time { get; set; }
        public DateTime Last_Open_Time { get; set; }
        public DateTime Last_Record_Time { get; set; }
        public int Type { get; set; }
        public DateTime? Updatetime { get; set; }
        public DateTime Createtime { get; set; }

    }
}
