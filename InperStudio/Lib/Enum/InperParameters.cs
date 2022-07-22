using Stylet;
using System.Collections.Generic;

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
