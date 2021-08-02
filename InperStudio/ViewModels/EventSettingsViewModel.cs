using HandyControl.Controls;
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
using System.Text;
using System.Threading;
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
        #endregion
        public EventSettingsViewModel(EventSettingsTypeEnum typeEnum)
        {
            @enum = typeEnum;
            EventType = typeEnum.ToString();
        }
        protected override void OnViewLoaded()
        {
            view = View as EventSettingsView;
            switch (@enum)
            {
                case EventSettingsTypeEnum.Marker:
                    break;
                case EventSettingsTypeEnum.Output:
                    view.Title = "OutPut";
                    view.conditionGrid.Visibility = Visibility.Visible;
                    break;
                default:
                    break;
            }
            //读取所有通道
            {
                foreach (DioChannel item in InpertProductConfig.GetAllNodes())
                {
                    markerChannels.Add(new EventChannel()
                    {
                        IsActive = false,
                        ChannelId = item.ChannelId,
                        Name = item.Name,
                        BgColor = InperColorHelper.ColorPresetList[item.ChannelId],
                        //Type = item.Type ?? ChannelTypeEnum.Input.ToString()
                    });
                }

                foreach (Channel item in InperGlobalClass.CameraSignalSettings.CameraChannels)
                {
                    var channel = new EventChannel()
                    {
                        IsActive = false,
                        ChannelId = item.ChannelId,
                        Name = item.Name,
                        DeltaF = item.DeltaF == 0 ? 5 : item.DeltaF,
                        BgColor = InperColorHelper.ColorPresetList[item.ChannelId],
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
                        Name = "Start",
                        BgColor = InperColorHelper.ColorPresetList[0],
                        Type = ChannelTypeEnum.Start.ToString()
                    });
                    markerChannels.Add(new EventChannel()
                    {
                        IsActive = false,
                        ChannelId = 1,
                        Name = "Stop",
                        BgColor = InperColorHelper.ColorPresetList[1],
                        Type = ChannelTypeEnum.Stop.ToString()
                    });
                    markerChannels.Add(new EventChannel()
                    {
                        IsActive = false,
                        ChannelId = -1,
                        Name = "Manual",
                        BgColor = InperColorHelper.ColorPresetList[0],
                        Type = ChannelTypeEnum.Manual.ToString()
                    });
                }

                ConditionsChannels.Add(new EventChannel()
                {
                    IsActive = false,
                    ChannelId = 0,
                    Name = "Start",
                    BgColor = InperColorHelper.ColorPresetList[0],
                    Type = ChannelTypeEnum.Start.ToString()
                });
                ConditionsChannels.Add(new EventChannel()
                {
                    IsActive = false,
                    ChannelId = 1,
                    Name = "Stop",
                    BgColor = InperColorHelper.ColorPresetList[1],
                    Type = ChannelTypeEnum.Stop.ToString()
                });
                ConditionsChannels.Add(new EventChannel()
                {
                    IsActive = false,
                    ChannelId = -1,
                    Name = "Manual",
                    BgColor = InperColorHelper.ColorPresetList[0],
                    Type = ChannelTypeEnum.Manual.ToString()
                });
            }
            //配置文件匹配  并设置当前可用通道
            foreach (EventChannelJson item in InperGlobalClass.EventSettings.Channels)
            {
                if (@enum == EventSettingsTypeEnum.Marker)
                {
                    if (item.Type == ChannelTypeEnum.Output.ToString())
                    {
                        MarkerChannels.Remove(MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId));
                    }
                    else
                    {
                        if (item.Type == ChannelTypeEnum.Manual.ToString())
                        {
                            EventChannel manual = new EventChannel
                            {
                                ChannelId = item.ChannelId,
                                BgColor = InperColorHelper.ColorPresetList[item.ChannelId],
                                Name = item.Name,
                                IsActive = item.IsActive
                            };

                            MarkerChannels.Add(manual);
                            manualChannels.Add(manual);
                        }
                        else
                        {
                            MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).IsActive = item.IsActive;
                        }
                    }
                }
                else
                {
                    if (item.Type == ChannelTypeEnum.Input.ToString())
                    {
                        EventChannel input = MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId);
                        if (input != null)
                        {
                            MarkerChannels.Remove(input);
                        }
                    }
                    if (item.Type == ChannelTypeEnum.Output.ToString())
                    {
                        MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).IsActive = item.IsActive;
                    }
                }

            }
            view.MarkerChannelCombox.SelectedItem = markerChannels.FirstOrDefault(x => x.IsActive == false);
            if(@enum == EventSettingsTypeEnum.Output)
            {
                view.ConditionChannelCombox.SelectedIndex = 0;
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
                    EventChannel ch = this.view.MarkerChannelCombox.SelectedItem as EventChannel;
                    if (ch.Name.StartsWith("DIO"))
                    {
                        if (tb.Text.Length < 6 || !tb.Text.StartsWith("DIO-" + (view.MarkerChannelCombox.SelectedIndex + 1) + "-"))
                        {
                            tb.Text = "DIO-" + (this.view.MarkerChannelCombox.SelectedIndex + 1) + "-";
                            tb.SelectionStart = tb.Text.Length;
                            Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Start.ToString())
                    {
                        if (tb.Text.Length < 5 || !tb.Text.StartsWith("Start"))
                        {
                            tb.Text = "Start";
                            tb.SelectionStart = tb.Text.Length;
                            Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Stop.ToString())
                    {
                        if (tb.Text.Length < 4 || !tb.Text.StartsWith("Stop"))
                        {
                            tb.Text = "Stop";
                            tb.SelectionStart = tb.Text.Length;
                            Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Manual.ToString())
                    {
                        if (tb.Text.Length < 7 || !tb.Text.StartsWith("Manual" + (manualChannels.Count == 0 ? 1 : manualChannels.Last().ChannelId + 2)))
                        {
                            tb.Text = "Manual" + (manualChannels.Count == 0 ? 1 : manualChannels.Last().ChannelId + 2);
                            tb.SelectionStart = tb.Text.Length;
                            Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Camera.ToString() && !ch.Name.StartsWith("DIO"))
                    {
                        if (tb.Text.Length < 6 || !tb.Text.StartsWith("ROI-" + ch.ChannelId + "-"))
                        {
                            tb.Text = "ROI-" + ch.ChannelId + "-";
                            tb.SelectionStart = tb.Text.Length;
                            Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                    }
                    if (ch.Type == ChannelTypeEnum.Analog.ToString())
                    {
                        if (tb.Text.Length < 5 || !tb.Text.StartsWith("AI-" + ch.ChannelId + "-"))
                        {
                            tb.Text = "AI-" + ch.ChannelId + "-";
                            tb.SelectionStart = tb.Text.Length;
                            Growl.Warning(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                            return;
                        }
                    }
                    MarkerChannels[view.MarkerChannelCombox.SelectedIndex].Name = tb.Text;
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
                MarkerChannels[view.MarkerChannelCombox.SelectedIndex].Hotkeys = bt.Content == null ? "" : bt.Content.ToString();
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
                object cb = (sender as System.Windows.Controls.ComboBox).SelectedItem;
                if (cb != null)
                {
                    var item = cb as EventChannel;
                    if (item.Type == ChannelTypeEnum.Manual.ToString())
                    {
                        view.MarkerName.Text = "Manual" + (manualChannels.Count == 0 ? 1 : manualChannels.Last().ChannelId + 2);
                        MarkerChannels[view.MarkerChannelCombox.SelectedIndex].BgColor = InperColorHelper.ColorPresetList[manualChannels.Count == 0 ? 0 : manualChannels.Last().ChannelId + 1];
                        MarkerChannels[view.MarkerChannelCombox.SelectedIndex].Hotkeys = "F" + (manualChannels.Count == 0 ? 1 : manualChannels.Last().ChannelId + 2);
                        view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(InperColorHelper.ColorPresetList[manualChannels.Count == 0 ? 0 : manualChannels.Last().ChannelId + 1]));
                    }
                    else
                    {
                        view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.BgColor));
                    }
                    view.hotkeys.Content = item.Hotkeys;
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
                MarkerChannels[view.MarkerChannelCombox.SelectedIndex].BgColor = (sender as ListBox).SelectedValue.ToString();
                this.view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString((sender as ListBox).SelectedValue.ToString())); ;
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
                EventChannel ch = this.view.MarkerChannelCombox.SelectedItem as EventChannel;
                EventChannel ch_active = this.view.markerActiveChannel.SelectedItem as EventChannel;
                if (moveType == "leftMove")//右移是激活 左移是取消激活
                {
                    if (ch_active != null)
                    {
                        if (ch.Type != ChannelTypeEnum.Manual.ToString())
                        {
                            MarkerChannels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && x.Type == ch_active.Type).IsActive = false;
                        }
                        var item = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId && x.Type == ch_active.Type);
                        if (item != null)
                        {
                            _ = InperGlobalClass.EventSettings.Channels.Remove(item);
                            if (item.Type == ChannelTypeEnum.Input.ToString())
                            {

                                Monitor.Enter(InperDeviceHelper.Instance._EventQLock);
                                IRenderableSeriesViewModel render = InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.FirstOrDefault(x => ((LineRenderableSeriesViewModel)x).Tag.ToString() == ch_active.ChannelId.ToString());
                                if (render != null)
                                {
                                    _ = InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.Remove(render);
                                    _ = InperDeviceHelper.Instance.EventChannelChart.EventQs.Remove(ch_active.ChannelId);
                                }
                                Monitor.Exit(InperDeviceHelper.Instance._EventQLock);
                            }
                            if (item.Type == ChannelTypeEnum.Manual.ToString())
                            {
                                _ = MarkerChannels.Remove(ch_active);
                                manualChannels.Remove(ch_active);
                            }
                        }
                    }
                }
                else
                {
                    if (ch != null)
                    {
                        if (ch.Type != ChannelTypeEnum.Manual.ToString())
                        {
                            MarkerChannels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Type == ch.Type).IsActive = true;
                        }

                        var item = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch.ChannelId && x.Type == ch.Type);

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

                            var channle = new EventChannelJson()
                            {
                                ChannelId = id,
                                IsActive = ch.IsActive,
                                Name = ch.Name,
                                BgColor = ch.BgColor,
                                DeltaF = ch.DeltaF,
                                Hotkeys = ch.Hotkeys,
                                Type = type
                            };

                            if (type == ChannelTypeEnum.Manual.ToString())
                            {
                                if (manualChannels.Count > 0)
                                {
                                    var manual = manualChannels.FirstOrDefault(x => x.Hotkeys == ch.Hotkeys);
                                    if (manual != null)
                                    {
                                        Growl.Warning(new GrowlInfo() { Message = "快捷键重复，请修改当前快捷键配置", Token = "SuccessMsg", WaitTime = 1 });
                                        return;
                                    }
                                }
                                channle.IsActive = true;
                                var chn = new EventChannel()
                                {
                                    ChannelId = id,
                                    IsActive = true,
                                    Name = ch.Name,
                                    BgColor = ch.BgColor,
                                    Hotkeys = ch.Hotkeys,
                                    Type = type
                                };
                                manualChannels.Add(chn);
                                MarkerChannels.Add(chn);
                            }
                            InperGlobalClass.EventSettings.Channels.Add(channle);
                        }
                        else
                        {
                            item.IsActive = ch.IsActive;
                        }
                        if (ch.Type == ChannelTypeEnum.Input.ToString())
                        {
                            Monitor.Enter(InperDeviceHelper.Instance._EventQLock);
                            InperDeviceHelper.Instance.EventChannelChart.RenderableSeries.Add(new LineRenderableSeriesViewModel() { Tag = ch.ChannelId, IsDigitalLine = true, DataSeries = new XyDataSeries<TimeSpan, double>(), Stroke = (Color)ColorConverter.ConvertFromString(ch.BgColor) });
                            InperDeviceHelper.Instance.EventChannelChart.EventQs.Add(ch.ChannelId, new Queue<KeyValuePair<long, double>>());
                            Monitor.Exit(InperDeviceHelper.Instance._EventQLock);
                        }
                        view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(MarkerChannels.FirstOrDefault(x => x.IsActive == false).BgColor));
                        view.MarkerChannelCombox.SelectedItem = MarkerChannels.FirstOrDefault(x => x.IsActive == false);
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        protected override void OnClose()
        {
            try
            {
                InperJsonHelper.SetEventSettings(InperGlobalClass.EventSettings);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion
    }
}
