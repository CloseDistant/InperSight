using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private BindableCollection<EventChannel> markerChannels = new BindableCollection<EventChannel>();
        public BindableCollection<EventChannel> MarkerChannels { get => markerChannels; set => SetAndNotify(ref markerChannels, value); }
        public List<string> EventColorList { get; set; } = InperColorHelper.ColorPresetList;
        #endregion
        public EventSettingsViewModel(EventSettingsTypeEnum typeEnum)
        {
            @enum = typeEnum;
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
                for (int i = 0; i < 8; i++)
                {
                    MarkerChannels.Add(new EventChannel() { IsActive = false, ChannelId = i, Name = "DIO-" + (i + 1) + "-PFC", BgColor = InperColorHelper.ColorPresetList[i], Hotkeys = InperColorHelper.HotkeysList[i] });
                }

                view.MarkerChannelCombox.SelectedItem = markerChannels.FirstOrDefault(x => x.IsActive == false);
            }
            //配置文件匹配  并设置当前可用通道
            foreach (EventChannelJson item in InperGlobalClass.EventSettings.Channels)
            {
                if (item.Type == @enum.ToString())
                {
                    MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).IsActive = item.IsActive;
                }
                else
                {
                    if (item.IsActive)
                    {
                        MarkerChannels.Remove(MarkerChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId));
                    }
                }
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
                    if (tb.Text.Length < 6 || !tb.Text.StartsWith("DIO-" + (view.MarkerChannelCombox.SelectedIndex + 1) + "-"))
                    {
                        tb.Text = "DIO-" + (this.view.MarkerChannelCombox.SelectedIndex + 1) + "-";
                        tb.SelectionStart = tb.Text.Length;
                        Growl.Error(new GrowlInfo() { Message = "固定字符串，请勿修改", Token = "SuccessMsg", WaitTime = 1 });
                        return;
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
                    bt.Content = bt.Content == null ? e.Key.ToString() : bt.Content + "+" + e.Key.ToString();
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
                    view.PopButton.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(item.BgColor));
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
                        markerChannels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId).IsActive = false;
                        var item = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch_active.ChannelId);
                        if (item != null)
                        {
                            _ = InperGlobalClass.EventSettings.Channels.Remove(item);
                        }
                    }
                }
                else
                {
                    if (ch != null)
                    {
                        markerChannels.FirstOrDefault(x => x.ChannelId == ch.ChannelId).IsActive = true;

                        var item = InperGlobalClass.EventSettings.Channels.FirstOrDefault(x => x.ChannelId == ch.ChannelId);

                        if (item == null)
                        {
                            InperGlobalClass.EventSettings.Channels.Add(new EventChannelJson()
                            {
                                ChannelId = ch.ChannelId,
                                IsActive = ch.IsActive,
                                Name = ch.Name,
                                BgColor = ch.BgColor,
                                DeltaF = ch.DeltaF,
                                Hotkeys = ch.Hotkeys,
                                Type = @enum.ToString()
                            });
                        }
                        else
                        {
                            item.IsActive = ch.IsActive;
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
