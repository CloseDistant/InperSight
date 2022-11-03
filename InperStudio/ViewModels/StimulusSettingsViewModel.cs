using InperProtocolStack.CmdPhotometry;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Lib.Chart;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudio.Views.Control;
using SciChart.Charting.Model.DataSeries;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace InperStudio.ViewModels
{
    public class StimulusSettingsViewModel : Screen
    {
        private readonly IWindowManager windowManager;
        private StimulusSettingsView view;
        private XyDataSeries<TimeSpan, double> xyDataSeries = new XyDataSeries<TimeSpan, double>();
        public XyDataSeries<TimeSpan, double> XyDataSeries
        {
            get => xyDataSeries;
            set => SetAndNotify(ref xyDataSeries, value);
        }
        private TimeSpanRange xRange = new TimeSpanRange(new TimeSpan(0), new TimeSpan(0, 0, 10));
        public TimeSpanRange XRange { get => xRange; set => SetAndNotify(ref xRange, value); }
        private ObservableCollection<WaveForm> waveForms = StimulusBeans.Instance.WaveForms;
        public ObservableCollection<WaveForm> WaveForms
        {
            get => waveForms;
            set => SetAndNotify(ref waveForms, value);
        }
        private ObservableCollection<Sweep> sweeps = StimulusBeans.Instance.Sweeps;
        public ObservableCollection<Sweep> Sweeps
        {
            get => sweeps;
            set => SetAndNotify(ref sweeps, value);
        }
        private ObservableCollection<EventChannel> eventChannels = new ObservableCollection<EventChannel>();
        public ObservableCollection<EventChannel> EventChannels { get => eventChannels; set => SetAndNotify(ref eventChannels, value); }
        public StimulusSettingsViewModel(IWindowManager _windowManager)
        {
            windowManager = _windowManager;
        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();

            view = View as StimulusSettingsView;
            view.CancelClickEvent += (e) =>
            {
                if (!StimulusBeans.Instance.IsConfigSweep)
                {
                    StimulusBeans.Instance.DioID = -1;
                }
            };
            view.ConfirmClickEvent += (s, e) =>
            {
                if (InperGlobalClass.IsStop)
                {
                    #region waveform 设置下发
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
                    #endregion
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
                    if (selectSweeps.Count == 0) { InperDeviceHelper.Instance.device.SetSweepState(0); StimulusBeans.Instance.DioID = -1; StimulusBeans.Instance.IsConfigSweep = false; }
                    InperGlobalClass.StimulusSettings.IsConfigSweep = StimulusBeans.Instance.IsConfigSweep;
                    InperGlobalClass.StimulusSettings.DioID = StimulusBeans.Instance.DioID;
                    InperGlobalClass.StimulusSettings.Hour = StimulusBeans.Instance.Hour;
                    InperGlobalClass.StimulusSettings.Minute = StimulusBeans.Instance.Minute;
                    InperGlobalClass.StimulusSettings.Seconds = StimulusBeans.Instance.Seconds;
                    InperJsonHelper.SetStimulusSettings(InperGlobalClass.StimulusSettings);
                    #endregion
                }
                this.RequestClose();
            };
            foreach (KeyValuePair<string, uint> item in InperDeviceHelper.Instance.device.DeviceIOIDs)
            {
                EventChannels.Add(new EventChannel()
                {
                    ChannelId = (int)item.Value,
                    SymbolName = item.Key.ToString(),
                    Name = item.Key.ToString(),
                });
            }
            foreach (EventChannelJson item in InperGlobalClass.EventSettings.Channels)
            {
                EventChannels.Remove(EventChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && item.SymbolName.StartsWith("DIO")));
            }
            view.dio.SelectedItem = StimulusBeans.Instance.DioID > 0 ? eventChannels.First(x => x.ChannelId == StimulusBeans.Instance.DioID) : eventChannels.First();
            view.dio.SelectionChanged += (s, e) =>
            {
                StimulusBeans.Instance.DioID = ((s as ComboBox).SelectedValue as EventChannel).ChannelId;
            };
            StimulusBeans.Instance.DioID = (view.dio.SelectedItem as EventChannel).ChannelId;
            view.hour.Text = StimulusBeans.Instance.Hour.ToString();
            view.minute.Text = StimulusBeans.Instance.Minute.ToString();
            view.seconds.Text = StimulusBeans.Instance.Seconds.ToString();
        }
        public void AddWaveformEvent()
        {
            try
            {
                this.windowManager.ShowDialog(new WaveformSettingViewModel());
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void AddSweepEvent()
        {
            try
            {
                this.windowManager.ShowDialog(new SweepSettingViewModel());
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #region waveform edit
        public void Edit_Event(WaveForm waveForm)
        {
            try
            {
                this.windowManager.ShowDialog(new WaveformSettingViewModel(waveForm));
                DrawChart();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Delete_Event(WaveForm waveForm)
        {
            try
            {
                bool isRefuse = false;
                StimulusBeans.Instance.Sweeps.ToList().ForEach(x =>
                {
                    if (x.WaveForm.Split(',').ToList().Contains(waveForm.Index.ToString()))
                    {
                        isRefuse = true;
                    }
                });
                if (isRefuse)
                {
                    MessageBox.Show("This kind of Waveform has been selected in Sweep!");
                    return;
                }
                InperDialogWindow inperDialogWindow = new InperDialogWindow("IsDelete?");
                inperDialogWindow.ClickEvent += (s, statu) =>
                {
                    if (statu == 0)
                    {
                        StimulusBeans.Instance.WaveForms.Remove(waveForm);
                        DrawChart();
                    }
                    else
                    {
                        inperDialogWindow.Close();
                    }
                };
                inperDialogWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion

        #region sweep edit
        public void Sweep_Edit_Event(Sweep sweep)
        {
            try
            {
                this.windowManager.ShowDialog(new SweepSettingViewModel(sweep));
                DrawChart();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Sweep_Delete_Event(Sweep sweep)
        {
            try
            {
                InperDialogWindow inperDialogWindow = new InperDialogWindow("IsDelete?");
                inperDialogWindow.ClickEvent += (s, statu) =>
                {
                    if (statu == 0)
                    {
                        StimulusBeans.Instance.Sweeps.Remove(sweep);
                        sweep.WaveForm.Split(',').ToList().ForEach(x =>
                        {
                            if (!string.IsNullOrEmpty(x))
                            {
                                StimulusBeans.Instance.WaveForms.FirstOrDefault(f => f.Index == int.Parse(x)).IsChecked = false;
                            }
                        });
                        if (selectSweeps.FirstOrDefault(x => x.Index == sweep.Index) is Sweep sweep1)
                        {
                            selectSweeps.Remove(sweep1);
                        }
                        DrawChart();
                    }
                    else
                    {
                        inperDialogWindow.Close();
                    }
                };
                inperDialogWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion

        List<Sweep> selectSweeps = new List<Sweep>();
        public void Sweep_Checked(object sen, RoutedEventArgs e)
        {
            try
            {
                Sweep sweep = (sen as CheckBox).DataContext as Sweep;
                sweep.IsChecked = true;
                if (selectSweeps.Count(x => x.Index == sweep.Index) == 0)
                {
                    selectSweeps.Add(sweep);
                    DrawChart();
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Sweep_Unchecked(object sen, RoutedEventArgs e)
        {
            try
            {
                Sweep sweep = (sen as CheckBox).DataContext as Sweep;
                sweep.IsChecked = false;
                if (selectSweeps.FirstOrDefault(x => x.Index == sweep.Index) is Sweep sweep1)
                {
                    selectSweeps.Remove(sweep1);
                }
                DrawChart();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void DrawChart()
        {
            List<WaveForm> _waveForms = new List<WaveForm>();
            selectSweeps.ToList().ForEach(x =>
            {
                x.WaveForm.Split(',').ToList().ForEach(w =>
                {
                    if (!string.IsNullOrEmpty(w))
                    {
                        WaveForms.ToList().ForEach(f =>
                        {
                            if (f.Index == int.Parse(w))
                            {
                                _waveForms.Add(f);
                            }
                        });
                    }
                });
            });
            TimeSpan timeSpan = new TimeSpan(StimulusBeans.Instance.Hour, StimulusBeans.Instance.Minute, StimulusBeans.Instance.Seconds);
            XyDataSeries.Clear();
            if (timeSpan.TotalSeconds > 0)
            {
                XyDataSeries = StimulusBeans.Instance.GetXyDataSeries(_waveForms, timeSpan.TotalSeconds);
                view.sciChart.ZoomExtents();
            }
        }
        private TextBoxAdorner BoxAdorner;
        public void Time_Changed(object s, TextChangedEventArgs e)
        {
            try
            {
                var tb = s as TextBox;
                var layer = AdornerLayer.GetAdornerLayer(tb);

                if (int.TryParse(tb.Text, out int res))
                {
                    if (tb.Name.Equals("hour"))
                    {
                        if (res >= 0 && res <= 99)
                        {
                            StimulusBeans.Instance.Hour = res;
                        }
                        else
                        {
                            if (BoxAdorner != null)
                            {
                                layer.Remove(BoxAdorner);
                            }

                            BoxAdorner = new TextBoxAdorner(tb, "无效的值");
                            layer.Add(BoxAdorner);
                            return;
                        }
                    }
                    else if (tb.Name.Equals("minute"))
                    {
                        if (res >= 0 && res < 60)
                        {
                            StimulusBeans.Instance.Minute = res;
                        }
                        else
                        {
                            if (BoxAdorner != null)
                            {
                                layer.Remove(BoxAdorner);
                            }

                            BoxAdorner = new TextBoxAdorner(tb, "无效的值");
                            layer.Add(BoxAdorner);
                            return;
                        }
                    }
                    else if (tb.Name.Equals("seconds"))
                    {
                        if (res >= 0 && res < 60)
                        {
                            StimulusBeans.Instance.Seconds = res;
                        }
                        else
                        {
                            if (BoxAdorner != null)
                            {
                                layer.Remove(BoxAdorner);
                            }

                            BoxAdorner = new TextBoxAdorner(tb, "无效的值");
                            layer.Add(BoxAdorner);
                            return;
                        }
                    }
                    if (BoxAdorner != null)
                    {
                        layer.Remove(BoxAdorner);
                    }
                }
                else
                {
                    if (BoxAdorner != null)
                    {
                        layer.Remove(BoxAdorner);
                    }

                    BoxAdorner = new TextBoxAdorner(tb, "无效的值");
                    layer.Add(BoxAdorner);
                }
                DrawChart();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
    }
}
