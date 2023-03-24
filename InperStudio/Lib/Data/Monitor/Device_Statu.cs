using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Monitor
{
    [SugarTable("device_statu", "设备在线状态", IsDisabledUpdateAll = true)]
    public class Device_Statu
    {
        [SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Snumber { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string Ip { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public DateTime? Updatetime { get; set; }
        public DateTime? Createtime { get; set; }
    }
}
