using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Bean.Stimulus
{
    public class Sweep
    {
        public bool IsChecked { get; set; }
        public int Index { get; set; }
        public string WaveForm { get; set; }
        public double Duration { get; set; }
    }
}
