using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static BindableCollection<double> FPS { get; set; } = new BindableCollection<double>()
        {
            6,
            12,
            18,
            30,
            42,
            60,
            90,
            120
        };
    }
}
