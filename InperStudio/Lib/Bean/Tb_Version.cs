using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Bean
{
    [SqlSugar.SugarTable("tb_version", "版本信息记录", IsDisabledDelete = true)]
    public class Tb_Version
    {
        [SqlSugar.SugarColumn(IsIdentity = true, IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Version_Name { get; set; }
        public string Version_Number { get; set; }
        public string Request_Id { get; set; }
        public string Desc { get; set; }
        public DateTime UploadTime { get; set; }
    }
}
