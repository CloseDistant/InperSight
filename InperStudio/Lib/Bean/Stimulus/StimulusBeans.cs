using InperProtocolStack.CmdPhotometry;
using InperStudio.Lib.Helper;
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

        public void StimulusCommandSend()
        {
            List<WaverformStruct> waverformStructs = new List<WaverformStruct>();
            InperGlobalClass.StimulusSettings.WaveForms = new List<WaveForm>();
            StimulusBeans.Instance.WaveForms.ToList().ForEach(x =>
            {
                WaverformStruct waverformStruct = new WaverformStruct()
                {
                    ID = x.Index,
                    WaveformType = 1,
                    PulseWidth = (float)x.Pulse,
                    Frequency = (float)x.Frequence,
                    Duration = x.Duration,
                    PowerRegionLow = 0,
                    PowerRegionHigh = 0,
                    EC_A = 0,
                    EC_B = 0
                };
                waverformStructs.Add(waverformStruct);
                InperGlobalClass.StimulusSettings.WaveForms.Add(x);
            });

            if (waverformStructs.Count > 0)
            {
                InperDeviceHelper.Instance.device.SetGBLWF(waverformStructs);

                waverformStructs.ForEach(w =>
                {
                    App.Log.Info("Waveform:" + w.ID + ":" + "---PulseWidth:" + w.PulseWidth + "---Frequency:" + w.Frequency + "---Duration:" + w.Duration + "---PowerRegionHigh:" + w.PowerRegionHigh + "---PowerRegionLow:" + w.PowerRegionLow + "---EC_A:" + w.EC_A + "---EC_B:" + w.EC_B);
                });
            }
 
            #region sweep设置下发
            CHNSweepStruct cHN = new CHNSweepStruct()
            {
                DioID = StimulusBeans.Instance.DioID,
                TotalTime = StimulusBeans.Instance.Hour * 3600 + StimulusBeans.Instance.Minute * 60 + StimulusBeans.Instance.Seconds,
                SweepStructs = new SweepStruct[StimulusBeans.Instance.Sweeps.Count(x => x.IsChecked)]
            };
            int count = 0;
            InperGlobalClass.StimulusSettings.Sweeps = new List<Sweep>();
            StimulusBeans.Instance.Sweeps.ToList().ForEach(x =>
            {
                InperGlobalClass.StimulusSettings.Sweeps.Add(x);
                if (x.IsChecked)
                {
                    var indexs = x.WaveForm.Split(',').ToList();
                    SweepStruct sweepStruct = new SweepStruct()
                    {
                        Duration = (float)x.Duration,
                        WaveformID = new int[indexs.Count]
                    };
                    sweepStruct.BasicWaveformCount = indexs.Count;
                    for (int i = 0; i < indexs.Count; i++)
                    {
                        sweepStruct.WaveformID[i] = int.Parse(indexs[i].ToString());
                    }

                    cHN.SweepStructs[count] = sweepStruct;
                    count++;
                }
            });
            if (cHN.SweepStructs.Length > 0)
            {
                List<byte> datas = new List<byte>();
                datas.AddRange(BitConverter.GetBytes(cHN.DioID));
                datas.AddRange(BitConverter.GetBytes(cHN.TotalTime));
                cHN.SweepStructs.ToList().ForEach(x =>
                {
                    datas.AddRange(BitConverter.GetBytes(x.Duration));
                    datas.AddRange(BitConverter.GetBytes(x.BasicWaveformCount));
                    x.WaveformID.ToList().ForEach(t =>
                    {
                        datas.AddRange(BitConverter.GetBytes(t));
                    });
                });

                InperDeviceHelper.Instance.device.SetCHNSweep(datas);
                InperDeviceHelper.Instance.device.SetSweepState(1);
                StimulusBeans.Instance.IsConfigSweep = true;
                StimulusBeans.Instance.Sweeps.ToList().ForEach(x =>
                {
                    if (x.IsChecked)
                    {
                        App.Log.Info("Sweep:" + x.Index + ":" + "-WaveForms:" + x.WaveForm + "-Duration:" + x.Duration);
                    }
                });
            }
            else
            {
                InperDeviceHelper.Instance.device.SetSweepState(0);
            }
         
            #endregion
        }
    }
}
