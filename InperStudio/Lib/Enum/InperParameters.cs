using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Enum
{
    public class InperParameters
    {
        public static List<double> Exposures { get; set; } = new List<double>()
        {
            1,
            6,
            10,
            12,
            15,
            20,
            30,
            40,
            50,
            300
        };
        public static List<double> FPS { get; set; } = new List<double>()
        {
            1,
            6,
            10,
            12,
            15,
            20,
            30,
            40,
            50,
            200
        };
    }
}
