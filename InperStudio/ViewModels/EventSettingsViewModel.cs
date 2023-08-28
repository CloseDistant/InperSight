using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Bean.Stimulus;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudioControlLib.Lib.Config;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Core.Extensions;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TextBox = HandyControl.Controls.TextBox;

namespace InperStudio.ViewModels
{
    public class EventSettingsViewModel : Screen
    {
        #region properties
        private readonly EventSettingsTypeEnum @enum;
        private EventSettingsView view;

        public string EventType { get; set; }
        private List<EventChannel> manualChannels = new List<EventChannel>();
        private BindableCollection<EventChannel> markerChannels = new BindableCollection<EventChannel>();
        public BindableCollection<EventChannel> MarkerChannels { get => markerChannels; set => SetAndNotify(ref markerChannels, value); }
        private BindableCollection<EventChannel> conditionsChannels = new BindableCollection<EventChannel>();
        public BindableCollection<EventChannel> ConditionsChannels { get => conditionsChannels; set => SetAndNotify(ref conditionsChannels, value); }
        public List<string> EventColorList { get; set; } = InperColorHelper.ColorPresetList;

        //public BindableCollection<EventChannelJson> ManualEvents = InperGlobalClass.ManualEvents;
        #endregion
        public EventSettingsViewModel(EventSettingsTypeEnum typeEnum)
        {
            @enum = typeEnum;
            EventType = typeEnum.ToString();
        }
        protected override void OnViewLoaded()
        {
            try
            {
                view = View as EventSettingsView;
                view.ConfirmClickEvent += (s, e) =>
                {
                    RequestClose();
                };
                switch (@enum)
                {
                    case EventSettingsTypeEnum.Marker:
                        if (InperConfig.Instance.Language.ToLower() == "en_us")
                        {
                            view.Title = "Marker";
                        }
                        else
                        {
                            view.Title = "打标";
                        }
                        break;
                    case EventSettingsTypeEnum.Output:
                        if (InperConfig.Instance.Language.ToLower() == "en_us")
                        {
                            view.Title = "Output";
                        }
                        else
                        {
                            view.Title = "输出";
                        }
                        view.MarkerColorList.SelectedIndex = 0;
                        view.conditionGrid.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
                //读取所有通道
                {
                    foreach (KeyValuePair<string, uint> item in InperDeviceHelper.Instance.device.DeviceIOIDs)
                    {
                        if ((int)item.Value != StimulusBeans.Instance.DioID || !StimulusBeans.Instance.IsConfigSweep)// 
                        {
                            if ((int)item.Value != StimulusBeans.Instance.TriggerId || !InperGlobalClass.StimulusSettings.IsTrigger)
                            {
                                markerChannels.Add(new EventChannel()
                                {
                                    IsActive = false,
                                    ChannelId = (int)item.Value,
                                    SymbolName = item.Key.ToString(),
                                    Name = item.Key.ToString(),
                                    BgColor = InperColorHelper.ColorPresetList[(int)item.Value],
                                    Type = ChannelTypeEnum.DIO.ToString()
                                });
                            }
                        }
                    }

                    foreach (Channel item in InperGlobalClass.CameraSignalSettings.CameraChannels)
                    {
                        EventChannel channel = new EventChannel()
                        {
                            IsActive = false,
                            ChannelId = item.ChannelId,
                            SymbolName = item.Name,
                            Name = item.Name,
                            DeltaF = item.DeltaF == 0 ? 5 : item.DeltaF,
                            Tau1 = item.Tau1,
                            Tau2 = item.Tau2,
                            Tau3 = item.Tau3,
                            BgColor = InperColorHelper.ColorPresetList[item.ChannelId > 100 ? item.ChannelId - 91 : item.ChannelId],
                            Type = item.Type ?? ChannelTypeEnum.Camera.ToString()
                        };
                        if (@enum == EventSettingsTypeEnum.Marker)
                        {
                            markerChannels.Add(channel);
                        }
                        ConditionsChannels.Add(channel);
                    }

                    if (@enum == EventSettingsTypeEnum.Marker)
                    {
                        markerChannels.Add(new EventChannel()
                        {
                            IsActive = false,
                            ChannelId = 17,
                            SymbolName = "Start",
                            Name = "Start",
                            BgColor = InperColorHelper.ColorPresetList[0],
                            Type = ChannelTypeEnum.Start.ToString()
                        });
                        markerChannels.Add(new EventChannel()
                        {
                            IsActive = false,
                            ChannelId = 18,
                            SymbolName = "Stop",
                            Name = "Stop",
                            BgColor = InperColorHelper.ColorPresetList[1],
                            Type = ChannelTypeEnum.Stop.ToString()
                        });
                        markerChannels.Add(new EventChannel()
                        {
                            IsActive = false,
                            ChannelId = -1,
                            SymbolName = "Manual",
                            Name = "Manual",
                            BgColor = InperColorHelper.ColorPresetList[0],
                            Type = ChannelTypeEnum.Manual.ToString()
                        });
                    }

                    ConditionsChannels.Add(new EventChannel()
                    {
                        IsActive = false,
                        ChannelId = 17,
                        SymbolName = "Start",
                        Name = "Start",
                        BgColor = InperColorHelper.ColorPresetList[0],
                        Type = ChannelTypeEnum.Start.ToString()
                    });
                    ConditionsChannels.Add(new EventChannel()
                    {
                        IsActive = false,
                        ChannelId = 18,
                        SymbolName = "Stop",
                        Name = "Stop",
                        BgColor = InperColorHelper.ColorPresetList[1],
                        Type = ChannelTypeEnum.Stop.ToString()
                    });
                    ConditionsChannels.Add(new EventChannel()
                    {
                        IsActive = false,
                        ChannelId = -1,
                        SymbolName = "Manual",
                        Name = "Manual",
                        BgColor = InperColorHelper.ColorPresetList[0],
                        Type = ChannelTypeEnum.Manual.ToString()
                    });
                }
                //配置文件匹配  并设置当前可用通道
                foreach (EventChannelJson item in InperGlobalClass.EventSettings.Channels)
                {
                    if (item.Type == ChannelTypeEnum.TriggerStart.ToString() || item.Type == ChannelTypeEnum.TriggerStop.ToString())
                    {
                        MarkerChannels.Remove(MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId));
                    }

                    if (item.Type == ChannelTypeEnum.Zone.ToString())
                    {

                        EventChannel zoneChannel = new EventChannel
                        {
                            ChannelId = item.ChannelId,
                            SymbolName = item.Name,
                            Name = item.Name,
                            IsActive = item.IsActive,
                            Type = item.Type,
                            VideoZone = item.VideoZone,
                        };
                        if (@enum == EventSettingsTypeEnum.Marker)
                        {
                            markerChannels.Add(zoneChannel);
                        }
                        ConditionsChannels.Add(zoneChannel);
                    }

                    if (@enum == EventSettingsTypeEnum.Marker)
                    {
                        if (item.Type == ChannelTypeEnum.Output.ToString())
                        {
                            MarkerChannels.Remove(MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == ChannelTypeEnum.DIO.ToString()));
                        }
                        else
                        {
                            if (item.Type == ChannelTypeEnum.Manual.ToString())
                            {
                                EventChannel manual = new EventChannel
                                {
                                    ChannelId = item.ChannelId,
                                    BgColor = item.BgColor,
                                    SymbolName = item.SymbolName,
                                    RefractoryPeriod = item.RefractoryPeriod,
                                    Hotkeys = item.Hotkeys,
                                    Name = item.Name,
                                    IsActive = item.IsActive,
                                    Type = item.Type
                                };

                                MarkerChannels.Add(manual);
                                manualChannels.Add(manual);
                            }
                            else
                            {
                                if (item.Type == ChannelTypeEnum.Input.ToString())
                                {
                                    EventChannel chn = MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && (x.Type == ChannelTypeEnum.DIO.ToString() || x.Type == ChannelTypeEnum.Input.ToString()));
                                    if (chn != null)
                                    {
                                        chn.IsActive = true; chn.Type = item.Type; chn.Name = item.Name; chn.BgColor = item.BgColor; chn.RefractoryPeriod = item.RefractoryPeriod;
                                    }
                                }
                                if (item.Type == ChannelTypeEnum.Enter.ToString() || item.Type == ChannelTypeEnum.Stay.ToString() || item.Type == ChannelTypeEnum.Leave.ToString() || item.Type == ChannelTypeEnum.EnterOrLeave.ToString())
                                {
                                    EventChannel _channel = new EventChannel
                                    {
                                        ChannelId = item.ChannelId,
                                        BgColor = item.BgColor,
                                        SymbolName = item.SymbolName,
                                        Name = item.Name,
                                        IsActive = item.IsActive,
                                        Type = item.Type,
                                        VideoZone = item.VideoZone,
                                    };
                                    markerChannels.Add(_channel);
                                }
                                else
                                {
                                    if (item.Type != ChannelTypeEnum.Zone.ToString())
                                    {
                                        EventChannel chn = MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type);
                                        if (chn != null)
                                        {
                                            chn.IsActive = item.IsActive; chn.BgColor = item.BgColor; chn.RefractoryPeriod = item.RefractoryPeriod;
                                            chn.DeltaF = item.DeltaF; chn.WindowSize = item.WindowSize;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (item.Type == ChannelTypeEnum.Input.ToString())
                        {
                            EventChannel input = MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && (x.Type == ChannelTypeEnum.DIO.ToString() || x.Type == ChannelTypeEnum.Input.ToString() || x.Type == ChannelTypeEnum.TriggerStart.ToString() || x.Type == ChannelTypeEnum.TriggerStop.ToString()));
                            if (input != null)
                            {
                                MarkerChannels.Remove(input);
                            }
                        }
                        if (item.Type == ChannelTypeEnum.Output.ToString())
                        {
                            //var chn = MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && (x.Type == ChannelTypeEnum.DIO.ToString() || x.Type == ChannelTypeEnum.Output.ToString()));
                            //chn.IsActive = item.IsActive; chn.Type = item.Type; chn.Name = item.Name;
                            MarkerChannels.Add(new EventChannel()
                            {
                                ChannelId = item.ChannelId,
                                Condition = item.Condition,
                                BgColor = item.BgColor,
                                DeltaF = item.DeltaF,
                                RefractoryPeriod = item.RefractoryPeriod,
                                LightIndex = item.LightIndex,
                                WindowSize = item.WindowSize,
                                IsActive = item.IsActive,
                                Name = item.Name,
                                SymbolName = item.SymbolName,
                            });
                        }
                    }

                }

                view.MarkerChannelCombox.SelectedItem = markerChannels.FirstOrDefault(x => x.IsActive == false);
                if (@enum == EventSettingsTypeEnum.Output)
                {
                    view.ConditionChannelCombox.SelectedIndex = 0;
                    view.ConditionsCombox.SelectedItem = markerChannels.FirstOrDefault(x => x.IsActive == false);
                }

                view.OutLightSources.ItemsSource = view.LightSources.ItemsSource = InperDeviceHelper.Instance.LightWaveLength.ToList().FindAll(x => x.IsChecked);
                if (InperDeviceHelper.Instance.LightWaveLength.ToList().FindAll(x => x.IsChecked).Count > 1)
                {
                    view.OutLightSources.SelectedIndex = view.LightSources.SelectedIndex = 1;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }

        #region method Marker
        public void MarkerName_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox tb = sender as TextBox;
                if (tb.IsFocused)
                {
                    EventChannel ch = @enum == EventSettingsTypeEnum.Marker
                        ? this.view.MarkerChannelCombox.SelectedItem as EventChannel
                        : this.view.ConditionsCombox.SelectedItem as EventChannel;
                    if (ch.Type == ChannelTypeEnum.Zone.ToString())
                    {
                        if (!tb.Text.StartsWith(ch.Name))
                        {
                            tb.Text = ch.Name + "-";
                            tb.SelectionStart = tb.Text.Length;
                        }
                    }
                    if (ch.Name.StartsWith("DIO") || ch.Type == ChannelTypeEnum.DIO.ToString())
                    {
                        if (tb.Text.Length < 6 || !tb.Text.StartsWith("DIO-" + (ch.ChannelId + 1) + "-"))
                        {
                            tb.Text = "DIO-" + (ch.ChannelId + 1) + "-";
                            tb.SelectionStart = tb.Text.Length;
                            //Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            //return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Start.ToString())
                    {
                        if (tb.Text.Length < 5 || !tb.Text.StartsWith("Start"))
                        {
                            tb.Text = "Start";
                            tb.SelectionStart = tb.Text.Length;
                            //Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            //return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Stop.ToString())
                    {
                        if (tb.Text.Length < 4 || !tb.Text.StartsWith("Stop"))
                        {
                            tb.Text = "Stop";
                            tb.SelectionStart = tb.Text.Length;
                            //Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            //return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Manual.ToString())
                    {
                        if (tb.Text.Length < 9 || !tb.Text.StartsWith("Manual-" + (manualChannels.Count == 0 ? 1 : manualChannels.Last().ChannelId + 2) + "-"))
                        {
                            tb.Text = "Manual-" + (manualChannels.Count == 0 ? 1 : manualChannels.Last().ChannelId + 2) + "-";
                            tb.SelectionStart = tb.Text.Length;
                            //Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            //return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Camera.ToString() && !ch.Name.StartsWith("DIO"))
                    {
                        if (tb.Text.Length < 6 || !tb.Text.StartsWith("ROI-" + (ch.ChannelId + 1) + "-"))
                        {
                            tb.Text = "ROI-" + (ch.ChannelId + 1) + "-";
                            tb.SelectionStart = tb.Text.Length;
                            //Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            //return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Analog.ToString())
                    {
                        if (tb.Text.Length < 5 || !tb.Text.StartsWith("AI-" + (ch.ChannelId - 100) + "-"))
                        {
                            tb.Text = "AI-" + (ch.ChannelId - 100) + "-";
                            tb.SelectionStart = tb.Text.Length;
                            //Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            //return;
                        }
                    }
                    if (ch.Type != ChannelTypeEnum.Zone.ToString())
                    {
                        MarkerChannels[view.MarkerChannelCombox.SelectedIndex].Name = tb.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Hotkeys_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var bt = sender as Button;
                if (e.Key == Key.Back)
                {
                    if (bt.Content != null)
                    {
                        bt.Content = bt.Content.ToString().Contains("+") ? bt.Content.ToString().Substring(0, bt.Content.ToString().LastIndexOf("+")) : null;
                    }
                }
                else
                {
                    if (bt.Content != null)
                    {
                        if (bt.Content.ToString().Split('+').Length < 3)
                        {
                            bt.Content = bt.Content == null ? e.Key.ToString() : bt.Content + "+" + e.Key.ToString();
                        }
                        else
                        {
                            Growl.Info(new GrowlInfo() { Message = "快捷键数量超出限制", Token = "SuccessMsg", WaitTime = 1 });
                        }
                    }
                    else
                    {
                        bt.Content = bt.Content == null ? e.Key.ToString() : bt.Content + "+" + e.Key.ToString();
                    }
                }
                if (@enum == EventSettingsTypeEnum.Marker)
                {
                    MarkerChannels[view.MarkerChannelCombox.SelectedIndex].Hotkeys = bt.Content == null ? "" : bt.Content.ToString();
                }
                else
                {
                    if (view.ConditionsCombox.SelectedItem is EventChannel chn)
                    {
                        chn.Condition.Hotkeys = bt.Content == null ? "" : bt.Content.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void PopButton_Click(object sender, RoutedEventArgs e)
        {
            this.view.pop.IsOpen = true;
        }
        public void MarkerChannelCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var comb = sender as System.Windows.Controls.ComboBox;

                if (comb.Visibility == Visibility.Collapsed || comb.Visibility == Visibility.Hidden)
                {
                    return;
                }

                object cb = comb.SelectedItem;
                if (cb != null)
                {
                    EventChannel item = cb as EventChannel;

                    view.MarkerName.Text = item.SymbolName;

                    if (item.Type == ChannelTypeEnum.Manual.ToString())
                    {
                        view.MarkerName.Text = "Manual-" + (manualChannels.Count == 0 ? 1 : manualChannels.Last().ChannelId + 2) + "-";
                        item.Name = "Manual";
                        MarkerChannels[view.MarkerChannelCombox.SelectedIndex].BgColor = InperColorHelper.ColorPresetList[manualChannels.Count == 0 ? 0 : manualChannels.Last().ChannelId + 1];
                        MarkerChannels[view.MarkerChannelCombox.SelectedIndex].Hotkeys = "F" + (manualChannels.Count == 0 ? 1 : manualChannels.Last().ChannelId + 2);
                        view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[manualChannels.Count == 0 ? 0 : manualChannels.Last().ChannelId + 1]));
                    }
                    else
                    {
                        view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.BgColor));
                        if (item.Type != ChannelTypeEnum.Start.ToString() || item.Type != ChannelTypeEnum.Stop.ToString())
                        {
                            view.MarkerName.Text += "-";
                        }
                    }
                    view.hotkeys.Content = item.Hotkeys;
                    view.lightSource.Visibility = Visibility.Visible;
                    if (@enum == EventSettingsTypeEnum.Output)
                    {
                        view.MarkerName.Text += view.ConditionChannelCombox.Text;
                        if ((view.ConditionChannelCombox.SelectedItem as EventChannel)?.Type == ChannelTypeEnum.Manual.ToString())
                        {
                            view.outputHotkeys.Content = (view.ConditionChannelCombox.SelectedItem as EventChannel).Hotkeys = "F" + (item.ChannelId + 1);
                        }
                        EventChannel con = view.ConditionChannelCombox.SelectedItem as EventChannel ?? null;
                        if (con != null)
                        {
                            item.Condition = new EventChannelJson()
                            {
                                ChannelId = con.ChannelId,
                                BgColor = con.BgColor,
                                Hotkeys = con.Hotkeys,
                                DeltaF = con.DeltaF,
                                WindowSize = con.WindowSize,
                                Name = con.Name,
                                Type = con.Type,
                                SymbolName = con.SymbolName,
                                VideoZone = con.VideoZone
                            };

                            if (item.Condition.Type == ChannelTypeEnum.Analog.ToString())
                            {
                                item.LightIndex = -1;
                            }
                            if (item.Condition.Type == ChannelTypeEnum.Camera.ToString())
                            {
                                if (view.OutLightSources.SelectedItem != null)
                                {
                                    item.LightIndex = (view.OutLightSources.SelectedItem as WaveGroup).GroupId;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (item.Type == ChannelTypeEnum.Camera.ToString())
                        {
                            item.LightIndex = (view.OutLightSources.SelectedItem as WaveGroup).GroupId;
                        }
                        if (item.Type == ChannelTypeEnum.Analog.ToString())
                        {
                            view.lightSource.Visibility = Visibility.Collapsed;
                            item.LightIndex = -1;
                        }
                        if (item.Type == ChannelTypeEnum.Zone.ToString())
                        {
                            view.markerMode.SelectedIndex = 0;
                            view.zoneConditions.SelectedIndex = 0;
                            view.MarkerName.Text = item.SymbolName + "-" + view.markerMode.Text + "-" + view.zoneConditions.Text;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void MarkerMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = view.MarkerChannelCombox.SelectedItem as EventChannel;
                if ((sender as System.Windows.Controls.ComboBox).SelectedItem == null)
                {
                    view.MarkerName.IsEnabled = true;
                    return;
                }
                var name = ((sender as System.Windows.Controls.ComboBox).SelectedItem as ZoneConditions).ZoneName;
                if (item.Type == ChannelTypeEnum.Zone.ToString() && @enum == EventSettingsTypeEnum.Marker)
                {
                    view.MarkerName.Text = item.SymbolName + "-" + name;
                    //view.MarkerName.IsEnabled = false;
                }
                else
                {
                    view.MarkerName.IsEnabled = true;
                }
                if (view.zoneConditions.SelectedItem == null)
                {
                    view.zoneConditions.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void ConditionsChannelCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                EventChannel item = (sender as System.Windows.Controls.ComboBox).SelectedItem as EventChannel;
                EventChannel channel = view.ConditionsCombox.SelectedItem as EventChannel;

                if (item != null && channel != null)
                {
                    view.MarkerName.Text = channel.SymbolName + "-" + item.Name;
                    if (item.Type == ChannelTypeEnum.Manual.ToString())
                    {
                        view.outputHotkeys.Content = item.Hotkeys = "F" + (channel.ChannelId + 1);
                    }
                    channel.Condition = new EventChannelJson()
                    {
                        ChannelId = item.ChannelId,
                        BgColor = item.BgColor,
                        Hotkeys = item.Hotkeys,
                        DeltaF = item.DeltaF,
                        WindowSize = item.WindowSize,
                        Tau1 = item.Tau1,
                        Tau2 = item.Tau2,
                        Tau3 = item.Tau3,
                        Name = item.Name,
                        Type = item.Type,
                        VideoZone = item.VideoZone,
                    };
                    if (item.Type == ChannelTypeEnum.Zone.ToString())
                    {
                        view.outputMode.SelectedIndex = 0;
                        view.zoneConditions1.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void MarkerColorList_Selected(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (@enum == EventSettingsTypeEnum.Marker)
                {
                    (view.MarkerChannelCombox.SelectedItem as EventChannel).BgColor = (sender as ListBox).SelectedValue.ToString();
                }
                else
                {
                    EventChannel chn = view.ConditionsCombox.SelectedItem as EventChannel;
                    if (chn != null)
                    {
                        chn.BgColor = (sender as ListBox).SelectedValue.ToString();
                    }
                }
                view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString((sender as ListBox).SelectedValue.ToString()));
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void MarkerMove(string moveType)
        {
            try
            {
                EventChannel ch = @enum == EventSettingsTypeEnum.Marker ? view.MarkerChannelCombox.SelectedItem as EventChannel : view.ConditionsCombox.SelectedItem as EventChannel;

                if (moveType == "leftMove")//右移是激活 左移是取消激活
                {
                    EventChannel ch_active = this.view.markerActiveChannel.SelectedItem as EventChannel;
                    if (ch_active != null)
                    {
                        ch_active.IsActive = false;
                        if (ch_active.Type != ChannelTypeEnum.Stay.ToString() && ch_active.Type != ChannelTypeEnum.Leave.ToString() && ch_active.Type != ChannelTypeEnum.EnterOrLeave.ToString() && ch_active.Type != ChannelTypeEnum.Enter.ToString() && ch_active.Type != ChannelTypeEnum.Zone.ToString() && ch_active.Type != ChannelTypeEnum.Manual.ToString() && @enum == EventSettingsTypeEnum.Marker)
                        {
                            EventChannel act = @enum == EventSettingsTypeEnum.Output
                                ? MarkerChannels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && x.Type == ch_active.Type && x.Condition.Type == ch_active.Condition.Type)
                                : MarkerChannels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && x.Type == ch_active.Type);
                            act.IsActive = false;
                            if (act.Type == ChannelTypeEnum.Start.ToString())
                            {
                                act.Name = ChannelTypeEnum.Start.ToString();
                            }
                            if (act.Type == ChannelTypeEnum.Stop.ToString())
                            {
                                act.Name = ChannelTypeEnum.Stop.ToString();
                            }
                            if (act.Type == ChannelTypeEnum.Input.ToString() || act.Type == ChannelTypeEnum.Output.ToString() || act.Type == ChannelTypeEnum.DIO.ToString())
                            {
                                act.Name = ChannelTypeEnum.DIO.ToString() + "-" + (act.ChannelId + 1);
                            }
                        }
                        EventChannelJson item = @enum == EventSettingsTypeEnum.Marker
                            ? InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && (x.Type == ch_active.Type) && x.Type != ChannelTypeEnum.Output.ToString())
                            : InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && (x.Type == ch_active.Type || x.Type == ChannelTypeEnum.Output.ToString()) && x.Condition?.ChannelId == ch_active.Condition?.ChannelId && x.Condition?.Type == ch_active.Condition?.Type);
                        if (item != null && item.Type != ChannelTypeEnum.Zone.ToString() && item.Type != ChannelTypeEnum.Leave.ToString() && item.Type != ChannelTypeEnum.Enter.ToString() && item.Type != ChannelTypeEnum.EnterOrLeave.ToString() && item.Type != ChannelTypeEnum.Stay.ToString())
                        {
                            _ = InperGlobalClass.EventSettings.Channels.Remove(item);
                            var r = InperDeviceHelper.Instance.DeltaFCalculateList.Remove(item);
                            if (item.Type == ChannelTypeEnum.Input.ToString())
                            {
                                IRenderableSeriesViewModel render = InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.FirstOrDefault(x => ((LineRenderableSeriesViewModel)x).Tag.ToString() == ch_active.ChannelId.ToString());
                                if (render != null)
                                {
                                    _ = InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.Remove(render);
                                    //_ = InperDeviceHelper.Instance.EventChannelChart.EventQs.TryRemove(ch_active.ChannelId);
                                }
                            }
                            if (item.Type == ChannelTypeEnum.Manual.ToString())
                            {
                                _ = MarkerChannels.Remove(ch_active);
                                manualChannels.RemoveWhere(x => x.Hotkeys == ch_active.Hotkeys && x.Type == ch_active.Type);
                                markerChannels.FirstOrDefault(x => x.ChannelId == -1 && x.Type == ChannelTypeEnum.Manual.ToString()).Name = ChannelTypeEnum.Manual.ToString();
                            }
                            else
                            //if (item.Type == ChannelTypeEnum.Output.ToString() || item.Type==ChannelTypeEnum.Input.ToString())
                            {
                                _ = MarkerChannels.Remove(ch_active);
                            }
                        }
                        else
                        {
                            if (ch_active.Type == ChannelTypeEnum.Zone.ToString() || ch_active.Type == ChannelTypeEnum.Leave.ToString() || ch_active.Type == ChannelTypeEnum.Enter.ToString() || ch_active.Type == ChannelTypeEnum.Stay.ToString() || ch_active.Type == ChannelTypeEnum.EnterOrLeave.ToString())
                            {
                                markerChannels.Remove(ch_active);
                                if (InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && x.SymbolName == ch_active.SymbolName) is EventChannelJson channelJson)
                                {
                                    InperGlobalClass.EventSettings.Channels.Remove(channelJson);
                                }
                            }
                        }
                    }
                    if (@enum == EventSettingsTypeEnum.Output)
                    {
                        if (ch_active.Condition.Type != ChannelTypeEnum.Leave.ToString() && ch_active.Condition.Type != ChannelTypeEnum.Stay.ToString() && ch_active.Condition.Type != ChannelTypeEnum.Enter.ToString())
                        {
                            view.ConditionsCombox.SelectedItem = MarkerChannels.FirstOrDefault(x => x.IsActive == false && x.ChannelId == ch_active.ChannelId);
                        }
                    }
                }
                else
                {
                    if (ch != null)
                    {
                        EventChannel mc = MarkerChannels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Type == ch.Type); //&& x.IsActive == false

                        if (ch.Type != ChannelTypeEnum.Manual.ToString())
                        {
                            //ch.IsActive = mc.IsActive = true;
                            mc.Name = view.MarkerName.Text;
                            if (mc.Name.EndsWith("-"))
                            {
                                mc.Name = mc.Name.Substring(0, mc.Name.Length - 1);
                            }
                        }

                        var outputChannels = InperGlobalClass.EventSettings.Channels.FindAll(x => x.Type == ChannelTypeEnum.Output.ToString());
                        //var lightChannels = outputChannels.FindAll(f => f.C hannelId == ch.ChannelId && (f.Condition.Type == ChannelTypeEnum.Light.ToString() || f.Condition.Type == ChannelTypeEnum.AfterExcitation.ToString()));

                        if (@enum == EventSettingsTypeEnum.Output)
                        {
                            EventChannelJson output = outputChannels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Condition?.Type == ch.Condition?.Type && (x.Type == ch.Type || x.Type == ChannelTypeEnum.Output.ToString()));// && x.Condition?.ChannelId == ch.Condition?.ChannelId

                            if (output != null && output.Condition.Type != ChannelTypeEnum.Manual.ToString())
                            {
                                string text = "条件已存在!";
                                if (InperConfig.Instance.Language == "en_us")
                                {
                                    text = "This condition already exists!";
                                }
                                Growl.Warning(text, "SuccessMsg");
                                return;
                            }
                        }
                        else
                        {
                            var markerChannels = InperGlobalClass.EventSettings.Channels.FindAll(x => x.Type != ChannelTypeEnum.Output.ToString());
                            var marekr = markerChannels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Type == ChannelTypeEnum.Input.ToString() && ch.Type != ChannelTypeEnum.Camera.ToString());
                            if (marekr == null)
                            {
                                markerChannels = markerChannels.FindAll(x => x.Type != ChannelTypeEnum.Input.ToString());
                                marekr = markerChannels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Type == ch.Type && ch.Type != ChannelTypeEnum.Zone.ToString());
                            }
                            if (marekr != null)
                            {
                                string text = "条件已存在!";
                                if (InperConfig.Instance.Language == "en_us")
                                {
                                    text = "This condition already exists!";
                                }
                                Growl.Warning(text, "SuccessMsg");
                                return;
                            }
                        }

                        EventChannelJson item = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Type == ch.Type);

                        if (item == null && ch.Type != ChannelTypeEnum.Zone.ToString())
                        {
                            string type = string.Empty;
                            type = @enum == EventSettingsTypeEnum.Marker
                                ? ch.Name.StartsWith("DIO") ? ChannelTypeEnum.Input.ToString() : ch.Type
                                : ch.Name.StartsWith("DIO") ? ChannelTypeEnum.Output.ToString() : ch.Type;
                            int id = ch.ChannelId;
                            if (type == ChannelTypeEnum.Manual.ToString())
                            {
                                id = manualChannels.Count == 0 ? 0 : manualChannels.Last().ChannelId + 1;
                            }
                            ch.RefractoryPeriod = double.Parse(view.refp.Text);
                            EventChannelJson channle = new EventChannelJson()
                            {
                                ChannelId = id,
                                IsActive = true,
                                SymbolName = ch.SymbolName,
                                Name = view.MarkerName.Text,
                                RefractoryPeriod = double.Parse(view.refp.Text),
                                BgColor = ch.BgColor,
                                DeltaF = ch.DeltaF,
                                WindowSize = ch.WindowSize,
                                Hotkeys = ch.Hotkeys,
                                Condition = JsonConvert.DeserializeObject<EventChannelJson>(JsonConvert.SerializeObject(ch.Condition)),
                                Type = type
                            };

                            if (channle.Name.EndsWith("-"))
                            {
                                channle.Name = channle.Name.Substring(0, channle.Name.Length - 1);
                            }
                            if (@enum == EventSettingsTypeEnum.Marker)
                            {
                                if (view.LightSources.SelectedItem != null)
                                {
                                    channle.LightIndex = (view.LightSources.SelectedItem as WaveGroup).GroupId;
                                }
                                if (channle.Type == ChannelTypeEnum.Camera.ToString() || channle.Type == ChannelTypeEnum.Analog.ToString())
                                {
                                    channle.IsRefractoryPeriod = true;
                                }
                                if (InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(l => l.ChannelId == channle.ChannelId && l.Type == channle.Type) is CameraChannel channel)
                                {
                                    channel.IsDeltaFCalculate = true;
                                }
                            }
                            else
                            {
                                if (view.OutLightSources.SelectedItem != null)
                                {
                                    channle.LightIndex = (view.OutLightSources.SelectedItem as WaveGroup).GroupId;
                                }
                                //output 热键重复排除
                                if (channle.Condition != null && channle.Condition.Type == ChannelTypeEnum.Manual.ToString())
                                {
                                    if (string.IsNullOrEmpty(channle.Condition.Hotkeys))
                                    {
                                        mc.IsActive = false;
                                        return;
                                    }
                                    bool isExit = false;
                                    InperGlobalClass.EventSettings.Channels.ForEach(x =>
                                    {
                                        if (x.Type == ChannelTypeEnum.Manual.ToString())
                                        {
                                            if (x.Hotkeys == channle.Condition.Hotkeys) //|| x.Name == channle.Condition.Name
                                            {
                                                string text = "条件已存在!";
                                                if (InperConfig.Instance.Language == "en_us")
                                                {
                                                    text = "This condition already exists!";
                                                }
                                                Growl.Warning(text, "SuccessMsg");
                                                isExit = true;
                                                return;
                                            }
                                        }
                                        if (x.Type == ChannelTypeEnum.Output.ToString() && x.Condition != null)
                                        {
                                            if (x.Condition.Type == ChannelTypeEnum.Manual.ToString())
                                            {
                                                if (x.Condition.Hotkeys == channle.Condition.Hotkeys || x.Name == channle.Name)
                                                {
                                                    string text = "条件已存在!";
                                                    if (InperConfig.Instance.Language == "en_us")
                                                    {
                                                        text = "This condition already exists!";
                                                    }
                                                    Growl.Warning(text, "SuccessMsg");
                                                    isExit = true;
                                                    return;
                                                }

                                            }
                                        }
                                    });
                                    if (isExit)
                                    {
                                        return;
                                    }
                                }
                                //output 不应期激活
                                if (channle.Condition != null && (channle.Condition.Type == ChannelTypeEnum.Camera.ToString() || channle.Condition.Type == ChannelTypeEnum.Analog.ToString()))
                                {
                                    channle.IsRefractoryPeriod = true;
                                }
                                if (InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(l => l.ChannelId == channle.Condition.ChannelId && l.Type == channle.Condition.Type) is CameraChannel channel)
                                {
                                    channel.IsDeltaFCalculate = true;
                                }
                            }

                            if (type == ChannelTypeEnum.Manual.ToString())
                            {
                                if (manualChannels.Count > 0)
                                {
                                    EventChannel manual = manualChannels.FirstOrDefault(x => x.Hotkeys == ch.Hotkeys || x.Name == ch.Name);
                                    if (manual != null)
                                    {
                                        string text = "条件已存在!";
                                        if (InperConfig.Instance.Language == "en_us")
                                        {
                                            text = "This condition already exists!";
                                        }
                                        Growl.Warning(text, "SuccessMsg");
                                        return;
                                    }
                                }
                                channle.IsActive = true;
                                EventChannel chn = new EventChannel()
                                {
                                    ChannelId = id,
                                    IsActive = true,
                                    Name = view.MarkerName.Text,
                                    BgColor = ch.BgColor,
                                    Hotkeys = ch.Hotkeys,
                                    Type = type
                                };
                                if (chn.Name.EndsWith("-"))
                                {
                                    chn.Name = chn.Name.Substring(0, chn.Name.Length - 1);
                                }

                                manualChannels.Add(chn);
                                //MarkerChannels.Add(chn);
                            }
                            //output zone条件配置
                            if (channle.Type == ChannelTypeEnum.Output.ToString())
                            {
                                channle.Condition.ChannelId = new Random().Next(999, 999999);

                                if (channle.Condition != null && view.outputzoneVisibility.IsVisible)
                                {
                                    if (InperGlobalClass.EventSettings.Channels.Count(x => x.Condition?.Name == ch.Condition.Name && x.ChannelId == channle.ChannelId && x.Type == ChannelTypeEnum.Output.ToString() && x.Condition.SymbolName == view.outputMode.Text && x.Condition.Type == view.zoneConditions1.Text) > 0)
                                    {
                                        string text = "条件已存在!";
                                        if (InperConfig.Instance.Language == "en_us")
                                        {
                                            text = "This condition already exists!";
                                        }
                                        ch.IsActive = mc.IsActive = false;
                                        Growl.Warning(text, "SuccessMsg");
                                        return;
                                    }
                                    channle.Condition.SymbolName = view.outputMode.Text;
                                    channle.Condition.Type = view.zoneConditions1.Text;
                                    channle.Condition.VideoZone = new VideoZone()
                                    {
                                        Name = ch.Condition.VideoZone.Name,
                                        AllZoneConditions = new List<ZoneConditions>()
                                    };
                                    var conditions = view.outputMode.SelectedValue as ZoneConditions;
                                    //conditions.Type = view.zoneConditions1.Text;
                                    int.TryParse(view.zoneDuration1.Text, out int duration);
                                    //conditions.Duration = duration;
                                    //conditions.IsImmediately = (bool)view.zoneImmediately1.IsChecked;
                                    //conditions.IsDelay = (bool)view.outputZone.IsChecked;
                                    int.TryParse(view.zoneDelaySeconds1.Text, out int delayDuration);
                                    channle.Condition.VideoZone.AllZoneConditions.Add(new ZoneConditions()
                                    {
                                        Color = conditions.Color,
                                        DelaySeconds = delayDuration,
                                        Duration = duration,
                                        IsImmediately = (bool)view.zoneImmediately1.IsChecked,
                                        IsDelay = (bool)view.outputZone.IsChecked,
                                        IsTimerSignal = conditions.IsTimerSignal,
                                        ShapeHeight = conditions.ShapeHeight,
                                        ShapeWidth = conditions.ShapeWidth,
                                        ShapeLeft = conditions.ShapeLeft,
                                        ShapeName = conditions.ShapeName,
                                        ShapeTop = conditions.ShapeTop,
                                        ShapeType = conditions.ShapeType,
                                        Timer = conditions.Timer,
                                        Type = view.zoneConditions1.Text,
                                        ZoneName = conditions.ZoneName,
                                    });
                                    //ch.IsActive = mc.IsActive = false;
                                    //channle.IsActive = true;
                                }
                            }
                            MarkerChannels.Add(JsonConvert.DeserializeObject<EventChannel>(JsonConvert.SerializeObject(channle)));

                            InperGlobalClass.EventSettings.Channels.Add(channle);
                            InperDeviceHelper.Instance.DeltaFCalculateList.Add(channle);
                        }
                        else  //如果已存在
                        {
                            if (item.Type == ChannelTypeEnum.Zone.ToString())
                            {
                                ch.IsActive = false;

                                var symboname = @enum == EventSettingsTypeEnum.Marker ? view.markerMode.Text : view.outputMode.Text;
                                //if (markerChannels.Count(x => x.SymbolName == symboname) > 0)
                                {
                                    //var zone = @enum == EventSettingsTypeEnum.Marker ? item.VideoZone.AllZoneConditions.FirstOrDefault(x => x.ZoneName == symboname && x.Type == view.zoneConditions.Text) : item.Condition.VideoZone.AllZoneConditions.FirstOrDefault(x => x.ZoneName == symboname && x.Type == view.zoneConditions.Text);
                                    var zone = @enum == EventSettingsTypeEnum.Marker ? InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.Name.StartsWith(ch.Name) && x.Type == view.zoneConditions.Text && x.SymbolName == symboname) :
                                        InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.Condition != null && x.Condition.Type == view.zoneConditions1.Text && x.Condition.SymbolName == symboname);
                                    if (zone != null)
                                    {
                                        //zone = new ZoneConditions()
                                        //{
                                        //    ZoneName = zone.ZoneName,
                                        //    Color = zone.Color,
                                        //    ShapeTop = zone.ShapeTop,
                                        //    ShapeLeft = zone.ShapeLeft,
                                        //    ShapeHeight = zone.ShapeHeight,
                                        //    ShapeWidth = zone.ShapeWidth,
                                        //    ShapeName = zone.ShapeName,
                                        //};
                                        string text = "条件已存在!";
                                        if (InperConfig.Instance.Language == "en_us")
                                        {
                                            text = "This condition already exists!";
                                        }
                                        Growl.Warning(text, "SuccessMsg");
                                        ch.IsActive = mc.IsActive = false;
                                        return;
                                    }
                                }
                                var chnjson = new EventChannel()
                                {
                                    Name = view.MarkerName.Text,
                                    IsActive = true,
                                    Type = item.Type,
                                    SymbolName = symboname,
                                    BgColor = ch.BgColor,
                                };
                                if (chnjson.Name.EndsWith("-"))
                                {
                                    chnjson.Name = chnjson.Name.Substring(0, chnjson.Name.Length - 1);
                                }
                                if (@enum == EventSettingsTypeEnum.Output)
                                {
                                    chnjson.Condition = item.Condition;
                                    chnjson.Type = ChannelTypeEnum.Output.ToString();
                                }
                                if (!view.outputzoneVisibility.IsVisible && !view.markerzoneVisibility.IsVisible)
                                {
                                    MarkerChannels.Add(chnjson);
                                }
                                EventChannelJson channle2 = new EventChannelJson
                                {
                                    ChannelId = chnjson.ChannelId,
                                    IsActive = chnjson.IsActive,
                                    SymbolName = chnjson.SymbolName,
                                    Name = view.MarkerName.Text,
                                    BgColor = chnjson.BgColor,
                                    Type = @enum == EventSettingsTypeEnum.Marker ? view.zoneConditions.Text : view.zoneConditions1.Text,
                                    VideoZone = new VideoZone()
                                    {
                                        Name = item.VideoZone.Name,
                                        AllZoneConditions = new List<ZoneConditions>()
                                    }
                                };
                                if (@enum == EventSettingsTypeEnum.Marker)
                                {
                                    var conditions = view.markerMode.SelectedValue as ZoneConditions;
                                    //conditions.Type = view.zoneConditions.Text;
                                    int.TryParse(view.zoneDuration.Text, out int duration);
                                    //conditions.Duration = duration;
                                    //conditions.IsImmediately = (bool)view.zoneImmediately.IsChecked;
                                    //conditions.IsDelay = (bool)view.markerZone.IsChecked;
                                    int.TryParse(view.zoneDelaySeconds.Text, out int delayDuration);
                                    //conditions.DelaySeconds = delayDuration;

                                    channle2.VideoZone.AllZoneConditions.Add(new ZoneConditions()
                                    {
                                        Color = conditions.Color,
                                        DelaySeconds = delayDuration,
                                        Duration = duration,
                                        IsImmediately = (bool)view.zoneImmediately.IsChecked,
                                        IsDelay = (bool)view.markerZone.IsChecked,
                                        IsTimerSignal = conditions.IsTimerSignal,
                                        ShapeHeight = conditions.ShapeHeight,
                                        ShapeWidth = conditions.ShapeWidth,
                                        ShapeLeft = conditions.ShapeLeft,
                                        ShapeName = conditions.ShapeName,
                                        ShapeTop = conditions.ShapeTop,
                                        ShapeType = conditions.ShapeType,
                                        Timer = conditions.Timer,
                                        Type = view.zoneConditions.Text,
                                        ZoneName = conditions.ZoneName,
                                    });
                                    channle2.ChannelId = new Random().Next(999, 999999);
                                }
                                else
                                {
                                    var conditions = view.outputMode.SelectedValue as ZoneConditions;
                                    //conditions.Type = view.zoneConditions1.Text;
                                    int.TryParse(view.zoneConditions1.Text, out int duration);
                                    //conditions.Duration = duration;
                                    //conditions.IsImmediately = (bool)view.zoneImmediately1.IsChecked;
                                    //conditions.IsDelay = (bool)view.outputZone.IsChecked;
                                    int.TryParse(view.zoneDelaySeconds1.Text, out int delayDuration);
                                    channle2.VideoZone.AllZoneConditions.Add(new ZoneConditions()
                                    {
                                        Color = conditions.Color,
                                        DelaySeconds = delayDuration,
                                        Duration = duration,
                                        IsImmediately = (bool)view.zoneImmediately1.IsChecked,
                                        IsDelay = (bool)view.outputZone.IsChecked,
                                        IsTimerSignal = conditions.IsTimerSignal,
                                        ShapeHeight = conditions.ShapeHeight,
                                        ShapeWidth = conditions.ShapeWidth,
                                        ShapeLeft = conditions.ShapeLeft,
                                        ShapeName = conditions.ShapeName,
                                        ShapeTop = conditions.ShapeTop,
                                        ShapeType = conditions.ShapeType,
                                        Timer = conditions.Timer,
                                        Type = view.zoneConditions1.Text,
                                        ZoneName = conditions.ZoneName,
                                    });
                                    channle2.Condition.ChannelId = new Random().Next(999, 999999);
                                }
                                ch.IsActive = mc.IsActive = false;
                                InperGlobalClass.EventSettings.Channels.Add(channle2);
                                MarkerChannels.Add(JsonConvert.DeserializeObject<EventChannel>(JsonConvert.SerializeObject(channle2)));
                            }
                            else
                            {
                                item.IsActive = ch.IsActive;
                            }
                        }


                        if ((ch.Type == ChannelTypeEnum.Input.ToString() || ch.Type == ChannelTypeEnum.DIO.ToString()) && @enum == EventSettingsTypeEnum.Marker)
                        {
                            InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = ch.ChannelId, IsDigitalLine = true, DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = (Color)ColorConverter.ConvertFromString(ch.BgColor) });
                            //InperDeviceHelper.Instance.EventChannelChart.EventQs.TryAdd(ch.ChannelId, new Queue<KeyValuePair<long, double>>());
                        }
                        view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(MarkerChannels.FirstOrDefault(x => x.IsActive == false)?.BgColor ?? "#000000"));

                        if (@enum == EventSettingsTypeEnum.Marker)
                        {
                            view.MarkerChannelCombox.SelectedItem = MarkerChannels.FirstOrDefault(x => x.IsActive == false);
                        }
                        else
                        {
                            //if (!view.outputzoneVisibility.IsVisible && !view.markerzoneVisibility.IsVisible)
                            //{
                            //    view.MarkerChannelCombox.SelectedItem = MarkerChannels.FirstOrDefault(x => x.IsActive == false);
                            //    EventChannel _item = new EventChannel()
                            //    {
                            //        BgColor = view.MarkerColorList.SelectedItem.ToString(),
                            //        ChannelId = ch.ChannelId,
                            //        Name = ch.SymbolName,
                            //        SymbolName = ch.SymbolName,
                            //        IsActive = false,
                            //        Condition = JsonConvert.DeserializeObject<EventChannelJson>(JsonConvert.SerializeObject(ch.Condition)),
                            //        Type = ch.Type
                            //    };
                            //    MarkerChannels.Add(_item);
                            //    view.ConditionsCombox.SelectedItem = _item;
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Test_MouseEnter(object sender, MouseEventArgs e)
        {
            try
            {
                EventChannel channel = (sender as Grid).DataContext as EventChannel;

                if (channel.Condition == null)
                {
                    if (channel.Type == "Camera")
                    {
                        (sender as Grid).ToolTip = new TextBlock() { Text = "δF/F:" + channel.DeltaF + " Windowsize:" + channel.WindowSize + " RefractoryPeriod:" + channel.RefractoryPeriod };
                    }
                    if (channel.Type == "Analog")
                    {
                        (sender as Grid).ToolTip = new TextBlock() { Text = "δF/F:" + channel.DeltaF + " Windowsize:" + channel.WindowSize };
                    }
                    if (channel.Type == "Manual")
                    {
                        (sender as Grid).ToolTip = new TextBlock() { Text = "hotkey:" + channel.Hotkeys };
                    }
                    if (channel.Type == ChannelTypeEnum.Zone.ToString() || channel.Type == ChannelTypeEnum.Enter.ToString() || channel.Type == ChannelTypeEnum.Stay.ToString() || channel.Type == ChannelTypeEnum.Leave.ToString())
                    {
                        //var chn = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.IsActive && x.SymbolName == channel.SymbolName);
                        var zone = channel.VideoZone.AllZoneConditions.FirstOrDefault(x => x.ZoneName == channel.SymbolName);
                        if (zone != null)
                        {
                            if (zone.IsImmediately)
                            {
                                (sender as Grid).ToolTip = new TextBlock() { Text = "Zone:" + channel.SymbolName + " Type:" + zone.Type };
                            }
                            if (zone.IsDelay)
                            {
                                (sender as Grid).ToolTip = new TextBlock() { Text = "Zone:" + channel.SymbolName + " Type:" + zone.Type + " Delay:" + zone.DelaySeconds };
                            }
                            if (zone.Type == ChannelTypeEnum.Stay.ToString())
                            {
                                (sender as Grid).ToolTip = new TextBlock() { Text = "Zone:" + channel.SymbolName + " Type:" + zone.Type + " Duration:" + zone.Duration };
                            }
                        }
                    }
                }
                else
                {
                    if (channel.Condition.Type == "Camera")
                    {
                        (sender as Grid).ToolTip = new TextBlock() { Text = "δF/F:" + channel.DeltaF + " Windowsize:" + channel.WindowSize + " RefractoryPeriod:" + channel.RefractoryPeriod };
                    }
                    if (channel.Condition.Type == "Analog")
                    {
                        (sender as Grid).ToolTip = new TextBlock() { Text = "δF/F:" + channel.DeltaF + " Windowsize:" + channel.WindowSize };
                    }
                    if (channel.Condition.Type == "Manual")
                    {
                        (sender as Grid).ToolTip = new TextBlock() { Text = "hotkey:" + channel.Condition.Hotkeys };
                    }
                    if (channel.Condition.Type == ChannelTypeEnum.Leave.ToString() || channel.Condition.Type == ChannelTypeEnum.Enter.ToString() || channel.Condition.Type == ChannelTypeEnum.Stay.ToString())
                    {
                        //var chn = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.IsActive && x.Condition != null && x.Condition.SymbolName == channel.Condition.SymbolName);
                        var zone = channel.Condition.VideoZone.AllZoneConditions.FirstOrDefault(x => x.ZoneName == channel.Condition.SymbolName);
                        if (zone != null)
                        {
                            if (zone.IsImmediately)
                            {
                                (sender as Grid).ToolTip = new TextBlock() { Text = "Zone:" + zone.ZoneName + " Type:" + channel.Condition.Type };
                            }
                            if (zone.IsDelay)
                            {
                                (sender as Grid).ToolTip = new TextBlock() { Text = "Zone:" + zone.ZoneName + " Type:" + channel.Condition.Type + " Delay:" + zone.DelaySeconds };
                            }
                            if (zone.Type == ChannelTypeEnum.Stay.ToString())
                            {
                                (sender as Grid).ToolTip = new TextBlock() { Text = "Zone:" + zone.ZoneName + " Type:" + channel.Condition.Type + " Duration:" + zone.Duration };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }

        public void ZoneConditions_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                var item = ((sender as System.Windows.Controls.ComboBox).SelectedItem as ComboBoxItem).Content;
                if (item != null)
                {
                    view.MarkerName.Text = view.MarkerName.Text.Substring(0, view.MarkerName.Text.LastIndexOf("-") + 1) + item.ToString();
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        #endregion
        protected override void OnClose()
        {
            int allCount = InperGlobalClass.EventSettings.Channels.Count(x => x.Type != ChannelTypeEnum.TriggerStart.ToString() && x.Type != ChannelTypeEnum.TriggerStop.ToString() && x.IsActive);
            if (allCount > 0)
            {
                int outputCount = InperGlobalClass.EventSettings.Channels.Count(x => x.Type == ChannelTypeEnum.Output.ToString());
                if (allCount > outputCount)
                {
                    InperGlobalClass.IsExistEvent = true;
                }
                else
                {
                    if (outputCount > 0)
                    {
                        int outputDioCount = InperGlobalClass.EventSettings.Channels.FindAll(x => x.Type == ChannelTypeEnum.Output.ToString() && x.Condition != null).Count(d => d.Condition.Type == ChannelTypeEnum.Light.ToString() || d.Condition.Type == ChannelTypeEnum.AfterExcitation.ToString());
                        if (outputCount > outputDioCount)
                        {
                            InperGlobalClass.IsExistEvent = true;
                        }
                        else
                        {
                            InperGlobalClass.IsExistEvent = false;
                        }
                    }
                }
            }
            else
            {
                InperGlobalClass.IsExistEvent = false;
            }
            //InperGlobalClass.IsExistEvent = InperGlobalClass.EventSettings.Channels.Count() > 0;

            //ManualEvents.Clear();
            //InperGlobalClass.EventSettings.Channels.ForEach(x =>
            //{
            //    //if (x.Name.EndsWith("-"))
            //    //{
            //    //    x.Name = x.Name.Substring(0, x.Name.Length - 1);
            //    //}
            //    if (x.IsActive)
            //    {
            //        if (x.Type == ChannelTypeEnum.Manual.ToString())
            //        {
            //            if (!ManualEvents.Contains(x))
            //            {
            //                ManualEvents.Add(x);
            //            }
            //        }
            //        if (x.Condition?.Type == ChannelTypeEnum.Manual.ToString() && x.Type == ChannelTypeEnum.Output.ToString())
            //        {
            //            if (!ManualEvents.Contains(x))
            //            {
            //                ManualEvents.Add(new EventChannelJson()
            //                {
            //                    ChannelId = x.ChannelId,
            //                    BgColor = x.BgColor,
            //                    Hotkeys = x.Condition.Hotkeys,
            //                    HotkeysCount = x.Condition.HotkeysCount,
            //                    Name = x.Name,
            //                    Type = x.Type,
            //                    IsActive = x.IsActive
            //                });
            //            }
            //        }
            //    }
            //});
            InperJsonHelper.SetEventSettings(InperGlobalClass.EventSettings);
        }
    }
}
