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
using System.Windows;
using System.Windows.Controls;

namespace InperStudio.ViewModels
{
    public class SignalPropertiesViewModel : Screen
    {
        #region filed
        private SignalPropertiesView view;
        private SignalPropertiesTypeEnum @enum;
        private int currentId;
        private readonly int _ChannleId = 999;
        public CameraSignalSettings CameraSignalSettings { get; set; } = InperGlobalClass.CameraSignalSettings;
        private BindableCollection<Channel> channels = new BindableCollection<Channel>();
        public BindableCollection<Channel> Channels { get => channels; set => SetAndNotify(ref channels, value); }
        #endregion
        public SignalPropertiesViewModel(SignalPropertiesTypeEnum @enum, int ChannelId)
        {
            this.@enum = @enum;
            currentId = ChannelId;
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
                        this.view.analog.Visibility = Visibility.Visible;
                        this.view.Title = "Analog Signal Properties";
                        break;
                    default:
                        break;
                }
                view.ConfirmClickEvent += (s, e) =>
                {
                    RequestClose();
                };
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void CameraInit()
        {
            this.view.camera.Visibility = Visibility.Visible;
            CameraSignalSettings.CameraChannels.ForEach(x =>
            {
                if (x.Filters == null)
                {
                    x.Filters = new Lib.Helper.JsonBean.Filters();
                }
                //if (x.Name.EndsWith("-"))
                //{
                //    x.Name = x.Name.Substring(0, x.Name.Length - 1);
                //}
                Channels.Add(x);
            });
            if (Channels.Count > 0)
            {
                if (CameraSignalSettings.AllChannelConfig == null)
                {
                    CameraSignalSettings.AllChannelConfig = new Channel();
                }
                CameraSignalSettings.AllChannelConfig.ChannelId = _ChannleId;
                CameraSignalSettings.AllChannelConfig.Name = "All";
                Channels.Add(CameraSignalSettings.AllChannelConfig);
            }
            view.rangeChannel.SelectedItem = view.offsetChannel.SelectedItem = view.filtersChannel.SelectedItem = channels.FirstOrDefault(x => x.ChannelId == currentId) ?? channels.First();
            view.cancle.IsEnabled = (bool)(channels.FirstOrDefault(x => x.ChannelId == currentId)?.Offset);
            view.offset.IsEnabled = (bool)!channels.FirstOrDefault(x => x.ChannelId == currentId)?.Offset;
            view.offsetChannel.SelectionChanged += (s, e) =>
            {
                var channel = (s as System.Windows.Controls.ComboBox).SelectedItem as Channel;
                view.offset.IsEnabled = !channel.Offset;
                view.cancle.IsEnabled = channel.Offset;
            };
        }
        public void WindowSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int value = int.Parse((sender as HandyControl.Controls.TextBox).Text);
                Channel item = view.offsetChannel.SelectedItem as Channel;
                if (item.ChannelId == _ChannleId)
                {
                    CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        x.OffsetWindowSize = value;
                    });
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        chn.OffsetWindowSize = value;
                    });
                    item.OffsetWindowSize = value;
                    CameraSignalSettings.AllChannelConfig.OffsetWindowSize = value;
                }
                else
                {
                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).OffsetWindowSize = value;
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        if (chn.ChannelId == item.ChannelId)
                        {
                            chn.OffsetWindowSize = value;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }

        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var item = this.view.rangeChannel.SelectedItem as Channel;

                var textbox = sender as System.Windows.Controls.TextBox;
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
                            foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                            {
                                channel.YVisibleRange.Max = value;
                            }
                            CameraSignalSettings.AllChannelConfig.YTop = value;
                        }
                        else
                        {
                            CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).YTop = value;
                            foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                            {
                                if (channel.ChannelId == item.ChannelId)
                                {
                                    channel.YVisibleRange.Max = value;
                                }
                            }
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
                            foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                            {
                                channel.YVisibleRange.Min = value;
                            }
                            CameraSignalSettings.AllChannelConfig.YBottom = value;
                        }
                        else
                        {
                            CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).YBottom = value;

                            foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                            {
                                if (channel.ChannelId == item.ChannelId)
                                {
                                    channel.YVisibleRange.Min = value;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Smooth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.view.filtersChannel.SelectedItem as Channel;
                if (item.ChannelId == _ChannleId)
                {
                    CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        x.Filters.IsSmooth = true;
                    });
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        chn.Filters.IsSmooth = true;
                    });
                    CameraSignalSettings.AllChannelConfig.Filters.IsSmooth = true;
                }
                else
                {
                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.IsSmooth = true;
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        if (chn.ChannelId == item.ChannelId)
                        {
                            chn.Filters.IsSmooth = true;
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Smooth_UnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.view.filtersChannel.SelectedItem as Channel;
                if (item.ChannelId == _ChannleId)
                {
                    CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        x.Filters.IsSmooth = false;
                    });
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        chn.Filters.IsSmooth = false;
                    });
                    CameraSignalSettings.AllChannelConfig.Filters.IsSmooth = false;
                }
                else
                {
                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.IsSmooth = false;
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        if (chn.ChannelId == item.ChannelId)
                        {
                            chn.Filters.IsSmooth = false;
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void Smooth_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var item = this.view.filtersChannel.SelectedItem as Channel;

                var textbox = sender as HandyControl.Controls.TextBox;
                double value = double.Parse(textbox.Text);
                if (item.ChannelId == _ChannleId)
                {
                    CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        x.Filters.Smooth = value;
                    });
                    foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                    {
                        channel.Filters.Smooth = value;
                        if (InperGlobalClass.IsPreview)
                        {
                            channel.LightModes.ForEach(x =>
                            {
                                InperDeviceHelper.Instance.FilterData[channel.ChannelId][x.LightType].Clear();
                            });
                        }
                    }
                    CameraSignalSettings.AllChannelConfig.Filters.Smooth = value;

                }
                else
                {
                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.Smooth = value;
                    foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                    {
                        if (channel.ChannelId == item.ChannelId)
                        {
                            channel.Filters.Smooth = value;
                            if (InperGlobalClass.IsPreview)
                            {
                                channel.LightModes.ForEach(x =>
                                {
                                    InperDeviceHelper.Instance.FilterData[channel.ChannelId][x.LightType].Clear();
                                });
                            }
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
                        foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                        {
                            channel.Offset = false;
                            if (InperGlobalClass.IsPreview)
                            {
                                channel.LightModes.ForEach(x =>
                                {
                                    InperDeviceHelper.Instance.OffsetData[channel.ChannelId][x.LightType].Clear();
                                    x.OffsetValue = 0;
                                });
                            }
                        }
                        CameraSignalSettings.AllChannelConfig.Offset = false;
                    }
                    else
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Offset = false;
                        _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                        {
                            if (chn.ChannelId == item.ChannelId)
                            {
                                chn.Offset = false;
                                if (InperGlobalClass.IsPreview)
                                {
                                    chn.LightModes.ForEach(x =>
                                    {
                                        InperDeviceHelper.Instance.OffsetData[chn.ChannelId][x.LightType].Clear();
                                        x.OffsetValue = 0;
                                    });
                                }
                            }
                        });
                    }
                    item.Offset = false;
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
                        CameraSignalSettings.AllChannelConfig.Offset = true;
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
                    item.Offset = true;
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
            InperJsonHelper.SetCameraSignalSettings(CameraSignalSettings);
        }
    }
}
