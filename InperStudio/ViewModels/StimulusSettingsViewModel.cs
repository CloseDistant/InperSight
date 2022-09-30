using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Lib.Chart;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudio.Views.Control;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace InperStudio.ViewModels
{
    public class StimulusSettingsViewModel : Screen
    {
        private readonly IWindowManager windowManager;
        private StimulusSettingsView view;
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
            view.ConfirmClickEvent += (s, e) =>
            {
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
            view.dio.SelectedItem = eventChannels.First();
            view.dio.SelectionChanged += (s, e) =>
            {
                StimulusBeans.Instance.DioID = ((s as ComboBox).SelectedValue as EventChannel).ChannelId;
            };
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
                InperDialogWindow inperDialogWindow = new InperDialogWindow("IsDelete?");
                inperDialogWindow.ClickEvent += (s, statu) =>
                {
                    if (statu == 0)
                    {
                        StimulusBeans.Instance.WaveForms.Remove(waveForm);
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
                            StimulusBeans.Instance.WaveForms.FirstOrDefault(f => f.Index == int.Parse(x)).IsChecked = false;
                        });
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
                        StimulusBeans.Instance.Hour = res;
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
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
    }
}
