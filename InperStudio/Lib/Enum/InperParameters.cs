using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Enum
{
    public class InperParameters
    {
        public static List<string> Exposures { get; set; } = new List<string>()
        {
            "0",
           "5",
           "10",
           "15",
           "20",
           "25",
           "30",
           "50"
        };
        public static List<double> FPS { get; set; } = new List<double>()
        {
            6,
            12,
            20,
            30,
            40,
            50,
            60,
            80,
            100,
            200
        };
    }
}
