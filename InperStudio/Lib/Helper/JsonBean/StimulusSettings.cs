using InperStudio.Lib.Bean.Stimulus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperStudio.Lib.Helper.JsonBean
{
    public class StimulusSettings
    {
        public int DioID { get; set; } = -1;
        public bool IsConfigSweep { get; set; } = false;
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Seconds { get; set; }
        public List<WaveForm> WaveForms { get; set; } = new List<WaveForm>();
        public List<Sweep> Sweeps { get; set; } = new List<Sweep>();
    }
}
