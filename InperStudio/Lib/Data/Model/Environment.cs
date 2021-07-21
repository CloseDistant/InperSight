﻿using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Model
{
    [SugarTable("Environment", "环境", IsDisabledUpdateAll = true)]
    public class Environment
    {
        [SugarColumn(IsIdentity = true,IsPrimaryKey = true)]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public DateTime CreateTime { get; set; }
    }
}