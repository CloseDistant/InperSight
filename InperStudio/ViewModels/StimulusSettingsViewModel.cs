using InperProtocolStack.CmdPhotometry;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Lib.Chart;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudio.Views.Control;
using InperStudioControlLib.Lib.Config;
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
            //view.CancelClickEvent += (e) =>
            //{
            //    if (!StimulusBeans.Instance.IsConfigSweep)
            //    {
            //        StimulusBeans.Instance.DioID = -1;
            //    }
            //};
            view.isUse.Unchecked += (s, e) =>
            {
                StimulusBeans.Instance.IsConfigSweep = false;
                StimulusBeans.Instance.DioID = -1;
                InperDeviceHelper.Instance.device.SetSweepState(0);
            };
            view.isUse.Checked += (s, e) =>
            {
                try
                {
                    StimulusBeans.Instance.DioID = (view.dio.SelectedValue as EventChannel).ChannelId;
                }
                catch (Exception ex)
                {
                    InperLogExtentHelper.LogExtent(ex, "StimulusSettingViewModel");
                }
            };
            view.ConfirmClickEvent += (s, e) =>
            {
                StimulusBeans.Instance.TriggerMode = view._triggerMode.SelectedIndex;
                StimulusBeans.Instance.IsTrigger = (bool)view._triggerToggle.IsChecked;
                StimulusBeans.Instance.IsActiveStimulus = (bool)view.isUse.IsChecked;
                //zzz
                if (view.dio.SelectedValue != null)
                {
                    StimulusBeans.Instance.DioID = (view.dio.SelectedValue as EventChannel).ChannelId;
                }
                if (InperGlobalClass.IsStop || InperGlobalClass.IsPreview)
                {
                    #region stimulus 设置下发
                    if ((bool)view.isUse.IsChecked)
                    {
                        if (StimulusBeans.Instance.DioID == -1)
                        {
                            string text = "无效的DIO";
                            if (InperConfig.Instance.Language == "en_us")
                            {
                                text = "No available DIO";
                            }
                            InperGlobalClass.ShowReminderInfo(text);
                            return;
                        }
                        StimulusBeans.Instance.StimulusCommandSend();
                        if (selectSweeps.Count == 0) { InperDeviceHelper.Instance.device.SetSweepState(0); StimulusBeans.Instance.DioID = -1; StimulusBeans.Instance.IsConfigSweep = false; }
                    }

                    InperGlobalClass.StimulusSettings.IsConfigSweep = StimulusBeans.Instance.IsConfigSweep;
                    InperGlobalClass.StimulusSettings.IsActiveStimulus = StimulusBeans.Instance.IsActiveStimulus;
                    InperGlobalClass.StimulusSettings.DioID = StimulusBeans.Instance.DioID;
                    InperGlobalClass.StimulusSettings.Hour = StimulusBeans.Instance.Hour;
                    InperGlobalClass.StimulusSettings.Minute = StimulusBeans.Instance.Minute;
                    InperGlobalClass.StimulusSettings.Seconds = StimulusBeans.Instance.Seconds;
                    InperGlobalClass.StimulusSettings.IsTrigger = StimulusBeans.Instance.IsTrigger;
                    if (InperGlobalClass.StimulusSettings.IsTrigger)
                    {
                        InperGlobalClass.StimulusSettings.TriggerId = StimulusBeans.Instance.TriggerId;
                        InperGlobalClass.StimulusSettings.TriggerMode = StimulusBeans.Instance.TriggerMode;
                    }
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
            if (EventChannels.Count > 0)
            {
                view.dio.SelectedItem = EventChannels.First();
                if (EventChannels.Count(x => x.ChannelId == StimulusBeans.Instance.DioID) > 0)
                {
                    view.dio.SelectedItem = EventChannels.First(x => x.ChannelId == StimulusBeans.Instance.DioID);
                }
                view.dio.SelectionChanged += (s, e) =>
                {
                    StimulusBeans.Instance.DioID = ((s as ComboBox).SelectedValue as EventChannel).ChannelId;
                    if (StimulusBeans.Instance.DioID == StimulusBeans.Instance.TriggerId && (bool)view._triggerToggle.IsChecked)
                    {
                        //InperGlobalClass.ShowReminderInfo("无法重复使用");
                        if (eventChannels.First(x => x.ChannelId != StimulusBeans.Instance.TriggerId) is EventChannel channel)
                        {
                            view._triggerdio.SelectedValue = channel;
                        }
                        else
                        {
                            view._triggerdio.SelectedIndex = -1;
                        }
                    }
                };
                StimulusBeans.Instance.DioID = (view.dio.SelectedItem as EventChannel).ChannelId;
                var chn = EventChannels.FirstOrDefault(x => x.ChannelId == StimulusBeans.Instance.TriggerId) ?? EventChannels.FirstOrDefault();
                view._triggerdio.SelectedItem = StimulusBeans.Instance.TriggerId > 0 ? chn : EventChannels.FirstOrDefault(x => x.ChannelId != StimulusBeans.Instance.DioID);
                view._triggerdio.SelectionChanged += (s, e) =>
                {
                    StimulusBeans.Instance.TriggerId = ((s as ComboBox).SelectedValue as EventChannel).ChannelId;
                    if (StimulusBeans.Instance.TriggerId == StimulusBeans.Instance.DioID)
                    {
                        if (eventChannels.First(x => x.ChannelId != StimulusBeans.Instance.DioID) is EventChannel channel)
                        {
                            view.dio.SelectedValue = channel;
                        }
                        else
                        {
                            view.dio.SelectedIndex = -1;
                        }
                    }
                };
                if (view._triggerdio.SelectedItem != null)
                {
                    StimulusBeans.Instance.TriggerId = (view._triggerdio.SelectedItem as EventChannel).ChannelId;
                }
            }
            view._triggerToggle.IsChecked = StimulusBeans.Instance.IsTrigger;
            view._triggerMode.SelectedIndex = StimulusBeans.Instance.TriggerMode;
            view.isUse.IsChecked = StimulusBeans.Instance.IsActiveStimulus;

            view.sweepsSource.ItemsSource = Sweeps;
        }
        public void AddWaveformEvent()
        {
            try
            {
                this.windowManager.ShowDialog(new WaveformSettingViewModel());
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                string text = "IsDelete?";
                if (InperConfig.Instance.Language != "en_us")
                {
                    text = "是否删除?";
                }
                InperDialogWindow inperDialogWindow = new InperDialogWindow(text);
                inperDialogWindow.HideCancleButton();
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
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #endregion

        #region sweep edit
        public void Sweep_Edit_Event(Sweep sweep)
        {
            try
            {
                var model = new SweepSettingViewModel(sweep);
                double second = sweep.Duration;
                model.SwepTimeChangeEvent += (s, e) =>
                {
                    TimeSpan timeSpan1 = new TimeSpan(StimulusBeans.Instance.Hour, StimulusBeans.Instance.Minute, StimulusBeans.Instance.Seconds);
                    var seconds = timeSpan1.TotalSeconds - second + e;
                    TimeSpan timeSpan = new TimeSpan(0, 0, (int)seconds < 0 ? 0 : (int)seconds);
                    view.hour.Text = timeSpan.Hours.ToString();
                    view.minute.Text = timeSpan.Minutes.ToString();
                    view.seconds.Text = timeSpan.Seconds.ToString();
                    DrawChart();
                };
                this.windowManager.ShowDialog(model);
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Sweep_Delete_Event(Sweep sweep)
        {
            try
            {
                string text = "IsDelete?";
                if (InperConfig.Instance.Language != "en_us")
                {
                    text = "是否删除?";
                }
                InperDialogWindow inperDialogWindow = new InperDialogWindow(text);
                inperDialogWindow.HideCancleButton();
                inperDialogWindow.ClickEvent += (s, statu) =>
                {
                    if (statu == 0)
                    {
                        StimulusBeans.Instance.Sweeps.Remove(sweep);
                        //sweep.WaveForm.Split(',').ToList().ForEach(x =>
                        //{
                        //    if (!string.IsNullOrEmpty(x))
                        //    {
                        //        StimulusBeans.Instance.WaveForms.FirstOrDefault(f => f.Index == int.Parse(x)).IsChecked = false;
                        //    }
                        //});
                        if (selectSweeps.FirstOrDefault(x => x.Index == sweep.Index) is Sweep sweep1)
                        {
                            selectSweeps.Remove(sweep1);
                        }
                        TimeSpan timeSpan1 = new TimeSpan(StimulusBeans.Instance.Hour, StimulusBeans.Instance.Minute, StimulusBeans.Instance.Seconds);
                        var seconds = timeSpan1.TotalSeconds - sweep.Duration;
                        TimeSpan timeSpan = new TimeSpan(0, 0, (int)seconds < 0 ? 0 : (int)seconds);
                        view.hour.Text = timeSpan.Hours.ToString();
                        view.minute.Text = timeSpan.Minutes.ToString();
                        view.seconds.Text = timeSpan.Seconds.ToString();
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
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                    TimeSpan timeSpan = new TimeSpan(0, 0, StimulusBeans.Instance.Seconds);
                    if ((sen as CheckBox).IsFocused)
                    {
                        timeSpan = new TimeSpan(0, 0, (int)sweep.Duration + StimulusBeans.Instance.Seconds);
                    }
                    view.hour.Text = (StimulusBeans.Instance.Hour + timeSpan.Hours).ToString();
                    view.minute.Text = (StimulusBeans.Instance.Minute + timeSpan.Minutes).ToString();
                    view.seconds.Text = timeSpan.Seconds.ToString();

                    selectSweeps.Add(sweep);
                    DrawChart();
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                    TimeSpan timeSpan1 = new TimeSpan(StimulusBeans.Instance.Hour, StimulusBeans.Instance.Minute, StimulusBeans.Instance.Seconds);
                    var seconds = timeSpan1.TotalSeconds - sweep1.Duration;
                    TimeSpan timeSpan = new TimeSpan(0, 0, (int)seconds < 0 ? 0 : (int)seconds);
                    view.hour.Text = timeSpan.Hours.ToString();
                    view.minute.Text = timeSpan.Minutes.ToString();
                    view.seconds.Text = timeSpan.Seconds.ToString();
                    selectSweeps.Remove(sweep1);
                }
                DrawChart();
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
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
                            string text = "无效的值";
                            if (InperConfig.Instance.Language == "en_us")
                            {
                                text = "Invalid value";
                            }
                            BoxAdorner = new TextBoxAdorner(tb, text);
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
                            string text = "无效的值";
                            if (InperConfig.Instance.Language == "en_us")
                            {
                                text = "Invalid value";
                            }
                            BoxAdorner = new TextBoxAdorner(tb, text);
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
                            string text = "无效的值";
                            if (InperConfig.Instance.Language == "en_us")
                            {
                                text = "Invalid value";
                            }
                            BoxAdorner = new TextBoxAdorner(tb, text);
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
                    string text = "无效的值";
                    if (InperConfig.Instance.Language == "en_us")
                    {
                        text = "Invalid value";
                    }
                    BoxAdorner = new TextBoxAdorner(tb, text);
                    layer.Add(BoxAdorner);
                }
                DrawChart();
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
    }
}
