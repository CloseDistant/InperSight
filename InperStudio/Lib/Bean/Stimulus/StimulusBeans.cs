using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InperStudio.Lib.Bean.Stimulus
{
    public class StimulusBeans : PropertyChangedBase
    {
        private static StimulusBeans stimulusBeans;

        public static StimulusBeans Instance
        {
            get
            {
                if (stimulusBeans == null)
                {
                    Interlocked.CompareExchange(ref stimulusBeans, new StimulusBeans(), null);
                }
                return stimulusBeans;
            }
        }

        private ObservableCollection<WaveForm> waveForms = new ObservableCollection<WaveForm>();
        public ObservableCollection<WaveForm> WaveForms
        {
            get => waveForms;
            set => SetAndNotify(ref waveForms, value);
        }
        private ObservableCollection<Sweep> sweeps = new ObservableCollection<Sweep>();
        public ObservableCollection<Sweep> Sweeps
        {
            get => sweeps;
            set => SetAndNotify(ref sweeps, value);
        }
        public int DioID { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Seconds { get; set; }
    }
}
