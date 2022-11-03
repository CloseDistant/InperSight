using SciChart.Charting.Model.DataSeries;
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
        public int DioID { get; set; } = -1;
        public bool IsConfigSweep { get; set; } = false;
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Seconds { get; set; }

        public XyDataSeries<TimeSpan, double> GetXyDataSeries(List<WaveForm> waves, double seconds)
        {
            try
            {
                if (seconds <= 0 || waves.Count == 0)
                {
                    return new XyDataSeries<TimeSpan, double>();
                }
                double duration = 0d;
                XyDataSeries<TimeSpan, double> xyDataSeries = new XyDataSeries<TimeSpan, double>();

                waves.ToList().ForEach(x =>
                {
                    int plusCount = (int)(x.Duration * x.Frequence);
                    double distance = 1.0 / x.Frequence;
                    if (plusCount > 0)
                    {
                        for (int i = 0; i < plusCount; i++)
                        {
                            duration += distance;
                            if (duration <= seconds)
                            {
                                using (xyDataSeries.SuspendUpdates())
                                {
                                    xyDataSeries.Append(new TimeSpan((long)(duration * Math.Pow(10, 7))), 0d);
                                    if (x.Pulse == 0)
                                    {
                                        xyDataSeries.Append(new TimeSpan((long)(duration * Math.Pow(10, 7))), 0d);
                                    }
                                    else
                                    {
                                        xyDataSeries.Append(new TimeSpan((long)(duration * Math.Pow(10, 7))), 1d);
                                    }
                                    xyDataSeries.Append(new TimeSpan((long)((duration * Math.Pow(10, 7)) + x.Pulse * Math.Pow(10, 4))), 0);
                                }
                            }
                        }
                    }
                });

                return xyDataSeries;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            return new XyDataSeries<TimeSpan, double>();
        }
    }
}
