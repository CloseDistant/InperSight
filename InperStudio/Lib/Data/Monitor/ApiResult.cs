using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Data.Monitor
{
    public class ApiResult
    {
        public bool Success { get; set; } = false;
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
