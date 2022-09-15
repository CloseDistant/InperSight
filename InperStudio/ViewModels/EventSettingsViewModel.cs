﻿using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        break;
                    case EventSettingsTypeEnum.Output:
                        view.Title = "Output";
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
                            ChannelId = 0,
                            SymbolName = "Start",
                            Name = "Start",
                            BgColor = InperColorHelper.ColorPresetList[0],
                            Type = ChannelTypeEnum.Start.ToString()
                        });
                        markerChannels.Add(new EventChannel()
                        {
                            IsActive = false,
                            ChannelId = 1,
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
                        ChannelId = 0,
                        SymbolName = "Start",
                        Name = "Start",
                        BgColor = InperColorHelper.ColorPresetList[0],
                        Type = ChannelTypeEnum.Start.ToString()
                    });
                    ConditionsChannels.Add(new EventChannel()
                    {
                        IsActive = false,
                        ChannelId = 1,
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
                    if (InperDeviceHelper.Instance.device.PhotometryInfo.IsOutput)
                    {
                        InperDeviceHelper.Instance.LightWaveLength.ToList().ForEach(x =>
                        {
                            ConditionsChannels.Add(new EventChannel()
                            {
                                IsActive = false,
                                ChannelId = x.GroupId,
                                SymbolName = x.WaveType.Split(' ').First(),
                                Name = x.WaveType.Split(' ').First(),
                                BgColor = InperColorHelper.ColorPresetList[0],
                                Type = ChannelTypeEnum.Light.ToString()
                            });
                        });

                        ConditionsChannels.Add(new EventChannel()
                        {
                            IsActive = false,
                            ChannelId = InperDeviceHelper.Instance.device.PhotometryInfo.OutputId,
                            SymbolName = "After-excitation",
                            Name = "After-excitation",
                            BgColor = InperColorHelper.ColorPresetList[0],
                            Type = ChannelTypeEnum.AfterExcitation.ToString()
                        });
                    }
                }
                //配置文件匹配  并设置当前可用通道
                foreach (EventChannelJson item in InperGlobalClass.EventSettings.Channels)
                {
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
                                else
                                {
                                    EventChannel chn = MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type);
                                    if (chn != null)
                                    {
                                        chn.IsActive = item.IsActive; chn.BgColor = item.BgColor; chn.RefractoryPeriod = item.RefractoryPeriod;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (item.Type == ChannelTypeEnum.Input.ToString())
                        {
                            EventChannel input = MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && (x.Type == ChannelTypeEnum.DIO.ToString() || x.Type == ChannelTypeEnum.Input.ToString()));
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
                App.Log.Error(ex.ToString());
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
                    //if (ch.Type != ChannelTypeEnum.Manual.ToString())
                    //{
                    MarkerChannels[view.MarkerChannelCombox.SelectedIndex].Name = tb.Text;
                    //}
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
                App.Log.Error(ex.ToString());
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
                    view.MarkerName.Text = item.Name;

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
                                Tau1 = con.Tau1,
                                Tau2 = con.Tau2,
                                Tau3 = con.Tau3,
                                Name = con.Name,
                                Type = con.Type
                            };
                            if (item.Condition.Type == ChannelTypeEnum.Analog.ToString())
                            {
                                item.LightIndex = -1;
                            }
                            if (item.Condition.Type == ChannelTypeEnum.Camera.ToString())
                            {
                                item.LightIndex = (view.OutLightSources.SelectedItem as WaveGroup).GroupId;
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
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void ConditionsChannelCombox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                EventChannel item = (sender as System.Windows.Controls.ComboBox).SelectedItem as EventChannel;
                EventChannel channel = view.ConditionsCombox.SelectedItem as EventChannel;
                if (item != null)
                {
                    view.MarkerName.Text = view.ConditionsCombox.Text + "-" + item.Name;
                    if (item.Type == ChannelTypeEnum.Manual.ToString())
                    {
                        view.outputHotkeys.Content = item.Hotkeys = "F" + (channel.ChannelId + 1);
                    }
                    if (channel != null)
                    {
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
                            Type = item.Type
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
                App.Log.Error(ex.ToString());
            }
        }
        public void MarkerMove(string moveType)
        {
            try
            {
                EventChannel ch = @enum == EventSettingsTypeEnum.Marker
                    ? view.MarkerChannelCombox.SelectedItem as EventChannel
                    : view.ConditionsCombox.SelectedItem as EventChannel;
                EventChannel ch_active = this.view.markerActiveChannel.SelectedItem as EventChannel;
                if (moveType == "leftMove")//右移是激活 左移是取消激活
                {
                    if (ch_active != null)
                    {
                        if (ch_active.Type != ChannelTypeEnum.Manual.ToString() && @enum == EventSettingsTypeEnum.Marker)
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
                            ? InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && (x.Type == ch_active.Type || x.Type == ChannelTypeEnum.Input.ToString()) && x.Type != ChannelTypeEnum.Output.ToString())
                            : InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && (x.Type == ch_active.Type || x.Type == ChannelTypeEnum.Output.ToString()) && x.Condition?.ChannelId == ch_active.Condition?.ChannelId && x.Condition?.Type == ch_active.Condition?.Type);
                        if (item != null)
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
                                manualChannels.Remove(ch_active);
                                markerChannels.FirstOrDefault(x => x.ChannelId == -1 && x.Type == ChannelTypeEnum.Manual.ToString()).Name = ChannelTypeEnum.Manual.ToString();
                            }
                            if (item.Type == ChannelTypeEnum.Output.ToString())
                            {
                                _ = MarkerChannels.Remove(ch_active);
                            }
                        }
                    }
                    if (@enum == EventSettingsTypeEnum.Output)
                    {
                        view.ConditionsCombox.SelectedItem = MarkerChannels.FirstOrDefault(x => x.IsActive == false && x.ChannelId == ch_active.ChannelId);
                    }
                }
                else
                {
                    if (ch != null)
                    {
                        EventChannel mc = MarkerChannels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Type == ch.Type && x.IsActive == false);
                        if (ch.Type != ChannelTypeEnum.Manual.ToString())
                        {
                            ch.IsActive = mc.IsActive = true;
                            mc.Name = view.MarkerName.Text;
                            if (mc.Name.EndsWith("-"))
                            {
                                mc.Name = mc.Name.Substring(0, mc.Name.Length - 1);
                            }
                        }

                        if (@enum == EventSettingsTypeEnum.Output)
                        {
                            if (InperGlobalClass.EventSettings.Channels.Count(x => x.Type == ChannelTypeEnum.Output.ToString()) > 0)
                            {
                                var items = InperGlobalClass.EventSettings.Channels.FindAll(x => x.Type == ChannelTypeEnum.Output.ToString());
                                if (ch.Type == ChannelTypeEnum.Light.ToString() || ch.Type == ChannelTypeEnum.AfterExcitation.ToString())
                                {
                                    if (items.FirstOrDefault(f => f.ChannelId == ch.ChannelId && (f.Condition.Type == ChannelTypeEnum.Light.ToString() || f.Condition.Type == ChannelTypeEnum.AfterExcitation.ToString())) is EventChannelJson channelJson)
                                    {
                                        Growl.Warning("This condition already exists!", "SuccessMsg");
                                        ch.IsActive = false;
                                        return;
                                    }
                                }
                            }
                            EventChannelJson output = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Condition?.Type == ch.Condition?.Type && x.Condition?.ChannelId == ch.Condition?.ChannelId && (x.Type == ch.Type || x.Type == ChannelTypeEnum.Output.ToString()));
                            if (output != null)
                            {
                                Growl.Warning("This condition already exists!", "SuccessMsg");
                                ch.IsActive = false;
                                return;
                            }
                        }

                        EventChannelJson item = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Type == ch.Type);

                        if (item == null)
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
                                IsActive = ch.IsActive,
                                SymbolName = ch.SymbolName,
                                Name = view.MarkerName.Text,
                                RefractoryPeriod = double.Parse(view.refp.Text),
                                BgColor = ch.BgColor,
                                DeltaF = ch.DeltaF,
                                WindowSize = ch.WindowSize,
                                Tau1 = ch.Tau1,
                                Tau2 = ch.Tau2,
                                Tau3 = ch.Tau3,
                                Hotkeys = ch.Hotkeys,
                                Condition = ch.Condition,
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
                                if (InperDeviceHelper.Instance._LoopCannels.FirstOrDefault(l => l.ChannelId == channle.ChannelId && l.Type == channle.Type) is CameraChannel channel)
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
                                    InperGlobalClass.EventSettings.Channels.ForEach(x =>
                                    {
                                        if (x.Type == ChannelTypeEnum.Manual.ToString())
                                        {
                                            if (x.Hotkeys == channle.Condition.Hotkeys || x.Name == channle.Condition.Name)
                                            {
                                                Growl.Warning(new GrowlInfo() { Message = "热键或名称重复", Token = "SuccessMsg", WaitTime = 1 });
                                                mc.IsActive = false;
                                                return;
                                            }
                                        }
                                        if (x.Type == ChannelTypeEnum.Output.ToString() && x.Condition != null)
                                        {
                                            if (x.Condition.Type == ChannelTypeEnum.Manual.ToString())
                                            {
                                                if (x.Condition.Hotkeys == channle.Condition.Hotkeys || x.Name == channle.Name)
                                                {
                                                    Growl.Warning(new GrowlInfo() { Message = "热键或名称重复", Token = "SuccessMsg", WaitTime = 1 });
                                                    mc.IsActive = false;
                                                }

                                            }
                                        }
                                    });
                                    if (mc.IsActive == false)
                                    {
                                        return;
                                    }
                                }
                                //output 不应期激活
                                if (channle.Condition != null && (channle.Condition.Type == ChannelTypeEnum.Camera.ToString() || channle.Condition.Type == ChannelTypeEnum.Analog.ToString()))
                                {
                                    channle.IsRefractoryPeriod = true;
                                }
                                if (InperDeviceHelper.Instance._LoopCannels.FirstOrDefault(l => l.ChannelId == channle.Condition.ChannelId && l.Type == channle.Condition.Type) is CameraChannel channel)
                                {
                                    channel.IsDeltaFCalculate = true;
                                }
                            }
                            if (type == ChannelTypeEnum.Camera.ToString() || type == ChannelTypeEnum.Analog.ToString())
                            {
                                _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                                  {
                                      if (chn.ChannelId == channle.ChannelId)
                                      {
                                          chn.LightModes.ForEach(x =>
                                          {
                                              x.Derivative = new Derivative(InperGlobalClass.CameraSignalSettings.Sampling, channle.Tau1, channle.Tau2, channle.Tau3);
                                          });
                                      }
                                  });
                            }

                            if (type == ChannelTypeEnum.Manual.ToString())
                            {
                                if (manualChannels.Count > 0)
                                {
                                    EventChannel manual = manualChannels.FirstOrDefault(x => x.Hotkeys == ch.Hotkeys || x.Name == ch.Name);
                                    if (manual != null)
                                    {
                                        Growl.Warning(new GrowlInfo() { Message = "热键或名称重复", Token = "SuccessMsg", WaitTime = 1 });
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
                                MarkerChannels.Add(chn);
                            }

                            InperGlobalClass.EventSettings.Channels.Add(channle);
                            InperDeviceHelper.Instance.DeltaFCalculateList.Add(channle);


                        }
                        else
                        {
                            item.IsActive = ch.IsActive;
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
                            view.MarkerChannelCombox.SelectedItem = MarkerChannels.FirstOrDefault(x => x.IsActive == false);
                            EventChannel _item = new EventChannel()
                            {
                                BgColor = view.MarkerColorList.SelectedItem.ToString(),
                                ChannelId = ch.ChannelId,
                                Name = ch.SymbolName,
                                SymbolName = ch.SymbolName,
                                IsActive = false,
                                Type = ch.Type
                            };
                            MarkerChannels.Add(_item);
                            view.ConditionsCombox.SelectedItem = _item;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion
        protected override void OnClose()
        {
            int allCount = InperGlobalClass.EventSettings.Channels.Count();
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
