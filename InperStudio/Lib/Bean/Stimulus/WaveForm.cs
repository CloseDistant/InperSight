using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Bean.Stimulus
{
    public class WaveForm
    {
        public bool IsChecked { get; set; }
        public int Index { get; set; }
        public ShapeEnum Shape { get; set; }
        public double Pulse { get; set; }
        public double Frequence { get; set; }
        public double Power { get; set; }
        public int Duration { get; set; }
    }

    public enum ShapeEnum
    {
        Constant,
        Square
    }
}
