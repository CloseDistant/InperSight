using System;

namespace InperStudio.Lib.Bean
{
    [SqlSugar.SugarTable("mcu_version", "mcu版本信息记录", IsDisabledDelete = true)]
    public class Mcu_Version
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
