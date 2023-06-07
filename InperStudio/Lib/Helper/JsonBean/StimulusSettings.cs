using InperStudio.Lib.Bean.Stimulus;
using System.Collections.Generic;

namespace InperStudio.Lib.Helper.JsonBean
{
    public class StimulusSettings
    {
        public int DioID { get; set; } = -1;
        public bool IsConfigSweep { get; set; } = false;
        public bool IsActiveStimulus { get; set; } = true;
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Seconds { get; set; }
        public int TriggerId { get; set; } = -1;
        public int TriggerMode { get; set; }
        public bool IsTrigger { get; set; }
        public List<WaveForm> WaveForms { get; set; } = new List<WaveForm>();
        public List<Sweep> Sweeps { get; set; } = new List<Sweep>();
    }
}
