using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
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
using System.Windows.Controls;

namespace InperStudio.ViewModels
{
    public class SignalPropertiesViewModel : Screen
    {
        #region filed
        private SignalPropertiesView view;
        private SignalPropertiesTypeEnum @enum;
        private readonly int _ChannleId = 999;
        public CameraSignalSettings CameraSignalSettings { get; set; } = InperGlobalClass.CameraSignalSettings;
        private BindableCollection<Channel> channels = new BindableCollection<Channel>();
        public BindableCollection<Channel> Channels { get => channels; set => SetAndNotify(ref channels, value); }
        #endregion
        public SignalPropertiesViewModel(SignalPropertiesTypeEnum @enum)
        {
            this.@enum = @enum;
        }
        protected override void OnViewLoaded()
        {
            try
            {
                this.view = this.View as SignalPropertiesView;
                switch (@enum)
                {
                    case SignalPropertiesTypeEnum.Camera:
                        CameraInit();
                        break;
                    case SignalPropertiesTypeEnum.Analog:
                        this.view.analog.Visibility = System.Windows.Visibility.Visible;
                        this.view.Title = "Analog Signal Properties";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void CameraInit()
        {
            this.view.camera.Visibility = System.Windows.Visibility.Visible;
            CameraSignalSettings.CameraChannels.ForEach(x =>
            {
                if (x.Filters == null)
                {
                    x.Filters = new Lib.Helper.JsonBean.Filters();
                }
                Channels.Add(x);
            });
            if (Channels.Count > 0)
            {
                Channels.Add(new Channel() { ChannelId = _ChannleId, Name = "All", YTop = 1, YBottom = 0.01, Filters = new Lib.Helper.JsonBean.Filters(), Offset = false });
            }
            view.offsetChannel.SelectionChanged += (s, e) =>
            {
                var channel = (s as System.Windows.Controls.ComboBox).SelectedItem as Channel;
                view.offset.IsEnabled = !channel.Offset;
                view.cancle.IsEnabled = channel.Offset;
            };
        }
        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var item = this.view.rangeChannel.SelectedItem as Channel;

                var textbox = sender as HandyControl.Controls.TextBox;
                double value = double.Parse(textbox.Text);

                if (textbox.Name.Contains("rangeTop"))
                {
                    if (value <= item.YBottom)
                    {
                        textbox.Text = (item.YBottom + 0.01).ToString();
                        Growl.Error(new GrowlInfo() { Message = "Top值不能小于Bottom值", Token = "SuccessMsg", WaitTime = 1 });
                    }
                    else
                    {
                        if (item.ChannelId == _ChannleId)
                        {
                            CameraSignalSettings.CameraChannels.ForEach(x =>
                            {
                                x.YTop = value;
                            });
                        }
                        else
                        {
                            CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).YTop = value;
                        }
                    }
                }
                else
                {
                    if (value >= item.YTop)
                    {
                        textbox.Text = (item.YTop - 0.01).ToString();
                        Growl.Error(new GrowlInfo() { Message = "Bottom值不能大于Top值", Token = "SuccessMsg", WaitTime = 1 });
                    }
                    else
                    {
                        if (item.ChannelId == _ChannleId)
                        {
                            CameraSignalSettings.CameraChannels.ForEach(x =>
                            {
                                x.YBottom = value;
                            });
                        }
                        else
                        {
                            CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).YBottom = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void CameraOffset(string type)
        {
            try
            {
                var item = this.view.offsetChannel.SelectedItem as Channel;
                if (type == "Cancle")
                {
                    if (item.ChannelId == _ChannleId)
                    {
                        CameraSignalSettings.CameraChannels.ForEach(x =>
                        {
                            x.Offset = false;
                        });
                        _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                        {
                            chn.Offset = false;
                        });
                        item.Offset = false;
                    }
                    else
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Offset = false;
                        _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                        {
                            if (chn.ChannelId == item.ChannelId)
                            {
                                chn.Offset = false;
                            }
                        });
                    }
                    view.offset.IsEnabled = true;
                    view.cancle.IsEnabled = false;
                }
                else
                {
                    if (item.ChannelId == _ChannleId)
                    {
                        CameraSignalSettings.CameraChannels.ForEach(x =>
                        {
                            x.Offset = true;
                        });
                        _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                          {
                              chn.Offset = true;
                          });
                        item.Offset = true;
                    }
                    else
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Offset = true;
                        _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                        {
                            if (chn.ChannelId == item.ChannelId)
                            {
                                chn.Offset = true;
                            }
                        });
                    }
                    view.cancle.IsEnabled = true;
                    view.offset.IsEnabled = false;
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
                InperJsonHelper.SetCameraSignalSettings(CameraSignalSettings);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
    }
}
