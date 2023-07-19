using Google.Protobuf.WellKnownTypes;
using HandyControl.Controls;
using HandyControl.Data;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using InperStudioControlLib.Control.TextBox;
using InperStudioControlLib.Lib.Config;
using MathNet.Numerics.Distributions;
using SciChart.Core.Extensions;
using Stylet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace InperStudio.ViewModels
{
    public class Light : PropertyChangedBase
    {
        public string LightName { get; set; }
        public int LightId { get; set; }
        public int ChannelId { get; set; } = -1;
        private string offsetValue;
        public string OffsetValue
        {
            get => offsetValue;
            set
            {
                SetAndNotify(ref offsetValue, value);
                if (InperDeviceHelper.Instance.CameraChannels.First(x => x.ChannelId == ChannelId) is var chn)
                {
                    if (double.TryParse(value.ToString(), out double num) && !LightName.IsNullOrEmpty())
                    {
                        chn.LightModes.First(f => f.LightType == LightId).OffsetValue = num;
                    }
                }
                InperGlobalClass.CameraSignalSettings.CameraChannels.ForEachDo(ch =>
                {
                    if (ch.ChannelId == ChannelId && !LightName.IsNullOrEmpty())
                    {
                        if (ch.LightOffsetValue.IsNullOrEmpty())
                        {
                            ch.LightOffsetValue = LightId.ToString() + ',' + value.ToString() + ' ';
                            return;
                        }
                        var res = ch.LightOffsetValue.Split(' ');
                        bool isExist = false; string saveStr = string.Empty;
                        res.ForEachDo(r =>
                        {
                            if (!r.IsNullOrEmpty())
                            {
                                if (r.Split(',')[0] == LightId.ToString())
                                {
                                    isExist = true;
                                    saveStr = LightId.ToString() + ',' + value.ToString() + ' ';
                                }
                                else
                                {
                                    saveStr += r + ' ';
                                }
                            }
                        });
                        if (!isExist)
                        {
                            saveStr = LightId.ToString() + ',' + value.ToString() + ' ';
                            ch.LightOffsetValue += saveStr;
                        }
                        else
                        {
                            ch.LightOffsetValue = saveStr;
                        }
                    }
                });
                Console.WriteLine(offsetValue);
            }
        }
    }
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
        private Channel activeChannel;
        public Channel ActiveChannel { get => activeChannel; set => SetAndNotify(ref activeChannel, value); }
        private BindableCollection<Light> lights = new BindableCollection<Light>();
        public BindableCollection<Light> Lights { get => lights; set => SetAndNotify(ref lights, value); }
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
                        this.view.camera.Visibility = Visibility.Visible;
                        CameraInit();
                        break;
                    case SignalPropertiesTypeEnum.Analog:
                        this.view.analog.Visibility = Visibility.Visible;
                        //this.view.ai_sampling.Visibility = Visibility.Visible;
                        CameraInit();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
            finally
            {
                view.ConfirmClickEvent += (s, e) =>
                {
                    this.view.Hide();
                    //RequestClose();
                };
            }
            base.OnViewLoaded();
        }
        private void CameraInit()
        {
            CameraSignalSettings.CameraChannels.ForEach(x =>
            {
                if (x.Filters == null)
                {
                    x.Filters = new Filters();
                }
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
                //if (@enum == SignalPropertiesTypeEnum.Camera)
                if (InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Camera.ToString() && x.ChannelId == currentId) is var chn)
                {
                    chn?.LightModes.OrderBy(m => m.LightType).ToList().ForEach(l =>
                    {
                        Light light = new Light()
                        {
                            LightId = l.LightType,
                            //ChannelId = currentId,
                            ChannelId = chn.ChannelId,
                        };
                        var lightWave = InperDeviceHelper.Instance.LightWaveLength.First(x => x.GroupId == l.LightType);
                        light.LightName = lightWave.WaveType;
                        light.OffsetValue = l.OffsetValue.ToString();
                        Lights.Add(light);
                    });
                }
            }

            ActiveChannel = Channels.FirstOrDefault(x => x.ChannelId == currentId);
            //view.heightChannel.SelectedItem = view.rangeChannel.SelectedItem = view.offsetChannel.SelectedItem = view.filtersChannel.SelectedItem = chn;
            //view.cancle.IsEnabled = (bool)(channels.FirstOrDefault(x => x.ChannelId == currentId)?.Offset);
            //view.offset.IsEnabled = (bool)!channels.FirstOrDefault(x => x.ChannelId == currentId)?.Offset;
            //view.selectChannel.SelectionChanged += (s, e) =>
            //{
            //    var channel = (s as System.Windows.Controls.ComboBox).SelectedItem as Channel;
            //    //view.offset.IsEnabled = !channel.Offset;
            //    //view.cancle.IsEnabled = channel.Offset;
            //};
        }
        public void RangeChannel_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                var comb = sender as System.Windows.Controls.ComboBox;

                var svalue = comb.SelectedValue as Channel;

                if (svalue.AutoRange)
                {
                    this.view.autoRange.IsChecked = true;
                    this.view.fixedRange.IsChecked = false;
                }
                else
                {
                    this.view.autoRange.IsChecked = false;
                    this.view.fixedRange.IsChecked = true;
                }
                if (svalue.Height.ToString() == "NaN")
                {
                    this.view.heightAuto.IsChecked = true;
                    this.view.heightFixed.IsChecked = false;
                }
                else
                {
                    this.view.heightAuto.IsChecked = false;
                    this.view.heightFixed.IsChecked = true;
                }

                if (svalue.ChannelId == _ChannleId)
                {
                    view._offsetTitle.Visibility = Visibility.Collapsed;
                    view._offsetValue.Visibility = Visibility.Collapsed;
                    view._offsetWindow.Visibility = Visibility.Collapsed;
                    view.lightMode.Visibility = Visibility.Collapsed;
                    view.aiOffsetValue.Visibility = Visibility.Collapsed;
                }
                else
                {
                    view._offsetTitle.Visibility = Visibility.Visible;
                    view._offsetValue.Visibility = Visibility.Visible;
                    view._offsetWindow.Visibility = Visibility.Visible;
                    if (svalue.Type == ChannelTypeEnum.Camera.ToString())
                    {
                        view.lightMode.Visibility = Visibility.Visible;
                        view.aiOffsetValue.Visibility = Visibility.Collapsed;

                        if (Lights.Count == 0)
                        {
                            if (InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.Type == ChannelTypeEnum.Camera.ToString() && x.ChannelId == svalue.ChannelId) is var _chn)
                            {
                                _chn?.LightModes.ForEach(l =>
                                {
                                    Light light = new Light()
                                    {
                                        LightId = l.LightType,
                                        //ChannelId = currentId,
                                        ChannelId = _chn.ChannelId,
                                    };
                                    var lightWave = InperDeviceHelper.Instance.LightWaveLength.First(x => x.GroupId == l.LightType);
                                    light.LightName = lightWave.WaveType;
                                    light.OffsetValue = l.OffsetValue.ToString();
                                    Lights.Add(light);
                                });
                            }
                        }
                    }
                    else
                    {
                        view.lightMode.Visibility = Visibility.Collapsed;
                        view.aiOffsetValue.Visibility = Visibility.Visible;
                    }
                    var chn = InperDeviceHelper.Instance.CameraChannels.First(x => x.ChannelId == svalue.ChannelId);
                    chn?.LightModes.ForEach(l =>
                        {
                            Lights.ForEachDo(x =>
                            {
                                if (x.LightId == l.LightType)
                                {
                                    x.ChannelId = svalue.ChannelId;
                                    x.OffsetValue = Math.Round(l.OffsetValue, 3).ToString();
                                }
                            });
                        });
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void AutoRange_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var rb = sender as RadioButton;
                Channel item = view.selectChannel.SelectedItem as Channel;
                bool isFixed = false;
                if (rb.Name.Equals("autoRange"))
                {
                    isFixed = true;
                }
                if (rb.Name.Equals("fixedRange"))
                {
                    isFixed = false;
                }
                if (item.ChannelId == _ChannleId)
                {
                    CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        x.AutoRange = isFixed;

                    });
                    InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(chn =>
                    {
                        chn.AutoRange = isFixed;
                        if (!isFixed)
                        {
                            chn.YVisibleRange.Max = ConverterString2Double(view.rangeTop.Text);
                            chn.YVisibleRange.Min = ConverterString2Double(view.rangeBottom.Text);
                            chn.YVisibleRange1.Max = ConverterString2Double(view.rangeTop.Text);
                            chn.YVisibleRange1.Min = ConverterString2Double(view.rangeBottom.Text);
                            chn.YVisibleRange2.Max = ConverterString2Double(view.rangeTop.Text);
                            chn.YVisibleRange2.Min = ConverterString2Double(view.rangeBottom.Text);
                            chn.YVisibleRange3.Max = ConverterString2Double(view.rangeTop.Text);
                            chn.YVisibleRange3.Min = ConverterString2Double(view.rangeBottom.Text);
                        }
                    });
                    CameraSignalSettings.AllChannelConfig.AutoRange = isFixed;
                }
                else
                {
                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).AutoRange = isFixed;
                    InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(chn =>
                    {
                        if (chn.ChannelId == item.ChannelId)
                        {
                            chn.AutoRange = isFixed;
                            chn.YVisibleRange.Max = ConverterString2Double(view.rangeTop.Text);
                            chn.YVisibleRange.Min = ConverterString2Double(view.rangeBottom.Text);
                            chn.YVisibleRange1.Max = ConverterString2Double(view.rangeTop.Text);
                            chn.YVisibleRange1.Min = ConverterString2Double(view.rangeBottom.Text);
                            chn.YVisibleRange2.Max = ConverterString2Double(view.rangeTop.Text);
                            chn.YVisibleRange2.Min = ConverterString2Double(view.rangeBottom.Text);
                            chn.YVisibleRange3.Max = ConverterString2Double(view.rangeTop.Text);
                            chn.YVisibleRange3.Min = ConverterString2Double(view.rangeBottom.Text);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        private double ConverterString2Double(string str)
        {
            if (str.IsNullOrEmpty()) return 0;
            double.TryParse(str, out double result);
            return result;
        }
        public void HeightChannel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var svalue = (sender as System.Windows.Controls.ComboBox).SelectedValue as Channel;

                if (svalue.Height.ToString() == "NaN")
                {
                    this.view.heightAuto.IsChecked = true;
                    this.view.heightFixed.IsChecked = false;
                }
                else
                {
                    this.view.heightAuto.IsChecked = false;
                    this.view.heightFixed.IsChecked = true;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void HeightAuto_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var rb = sender as RadioButton;
                Channel item = view.selectChannel.SelectedItem as Channel;
                if (rb.Name.Equals("heightAuto"))
                {
                    if (item.ChannelId == _ChannleId)
                    {
                        CameraSignalSettings.CameraChannels.ForEach(x =>
                        {
                            x.Height = double.NaN;
                        });
                        _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                        {
                            chn.Height = double.NaN;
                        });
                        CameraSignalSettings.AllChannelConfig.Height = double.NaN;
                    }
                    else
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Height = double.NaN;
                        InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(chn =>
                        {
                            if (chn.ChannelId == item.ChannelId)
                            {
                                chn.Height = double.NaN;
                            }
                        });
                    }
                }
                if (rb.Name.Equals("heightFixed"))
                {
                    if (item.Height.ToString() == "NaN" || item.Height == 0)
                    {
                        foreach (System.Windows.Window window in System.Windows.Application.Current.Windows)
                        {
                            if (window.Name.Contains("MainWindow"))
                            {
                                if (item.ChannelId == _ChannleId)
                                {
                                    CameraSignalSettings.CameraChannels.ForEach(x =>
                                    {
                                        x.Height = Math.Ceiling((((window.DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).View as DataShowControlView).dataScroll.ActualHeight / (Channels.Count - 1));
                                    });
                                    InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(chn =>
                                     {
                                         chn.Height = Math.Ceiling((((window.DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).View as DataShowControlView).dataScroll.ActualHeight / (Channels.Count - 1));
                                     });
                                    CameraSignalSettings.AllChannelConfig.Height = Math.Ceiling((((window.DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).View as DataShowControlView).dataScroll.ActualHeight / (Channels.Count - 1));
                                }
                                else
                                {
                                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type).Height = double.NaN;
                                    InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(chn =>
                                    {
                                        if (chn.ChannelId == item.ChannelId && chn.Type == item.Type)
                                        {
                                            chn.Height = Math.Ceiling((((window.DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).View as DataShowControlView).dataScroll.ActualHeight / (Channels.Count - 1));
                                        }
                                    });
                                }

                                this.view.fixedValue.Text = Math.Ceiling((((window.DataContext as MainWindowViewModel).ActiveItem as DataShowControlViewModel).View as DataShowControlView).dataScroll.ActualHeight / (Channels.Count - 1)).ToString();
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
        public void FixedValue_InperTextChanged(object arg1, TextChangedEventArgs arg2)
        {
            try
            {
                var tb = arg1 as InperStudioControlLib.Control.TextBox.InperTextBox;
                //if (tb.IsFocused)
                {
                    Regex rx = new Regex(@"^[+-]?\d*[.]?\d*$");
                    if (rx.IsMatch(tb.Text))
                    {
                        Channel item = view.selectChannel.SelectedItem as Channel;
                        double value = double.Parse(tb.Text);
                        if (value > 999)
                        {
                            value = 999;
                            tb.Text = "999";
                            string text = "最大值是999!";
                            if (InperConfig.Instance.Language == "en_us")
                            {
                                text = "The maximum is 999!";
                            }
                            InperGlobalClass.ShowReminderInfo(text);
                        }
                        if (item.ChannelId == _ChannleId)
                        {
                            CameraSignalSettings.CameraChannels.ForEach(x =>
                            {
                                x.Height = Math.Ceiling(value);
                            });
                            InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(chn =>
                            {
                                chn.Height = Math.Ceiling(value);
                            });
                            CameraSignalSettings.AllChannelConfig.Height = Math.Ceiling(value);
                        }
                        else
                        {
                            CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type).Height = Math.Ceiling(value);
                            InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(chn =>
                            {
                                if (chn.ChannelId == item.ChannelId && chn.Type == item.Type)
                                {
                                    chn.Height = Math.Ceiling(value);
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void WindowSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int value = int.Parse((sender as HandyControl.Controls.TextBox).Text);
                Channel item = view.selectChannel.SelectedItem as Channel;
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
                    //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                    //{
                    //    chn.OffsetWindowSize = value;
                    //});
                    item.OffsetWindowSize = value;
                    CameraSignalSettings.AllChannelConfig.OffsetWindowSize = value;
                }
                else
                {
                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type).OffsetWindowSize = value;
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        if (chn.ChannelId == item.ChannelId && chn.Type == item.Type)
                        {
                            chn.OffsetWindowSize = value;
                        }
                    });
                    //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                    //{
                    //    if (chn.ChannelId == item.ChannelId && chn.Type == item.Type)
                    //    {
                    //        chn.OffsetWindowSize = value;
                    //    }
                    //});
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Range_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.view.selectChannel.SelectedItem as Channel;

                var textbox = sender as System.Windows.Controls.TextBox;
                double value = double.Parse(textbox.Text);

                if (textbox.Name.Contains("rangeTop"))
                {
                    if (value <= item.YBottom)
                    {
                        textbox.Text = (item.YBottom + 0.01).ToString();
                        string text = "Top值不能小于Bottom值";
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "The Top value cannot be less than the Bottom value";
                        }
                        Growl.Warning(new GrowlInfo() { Message = text, Token = "SuccessMsg", WaitTime = 1 });
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
                                channel.YVisibleRange1.Max = value;
                                channel.YVisibleRange2.Max = value;
                                channel.YVisibleRange3.Max = value;
                            }
                            CameraSignalSettings.AllChannelConfig.YTop = value;
                        }
                        else
                        {
                            CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type).YTop = value;
                            foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                            {
                                if (channel.ChannelId == item.ChannelId && channel.Type == item.Type)
                                {
                                    channel.YVisibleRange.Max = value;
                                    channel.YVisibleRange1.Max = value;
                                    channel.YVisibleRange2.Max = value;
                                    channel.YVisibleRange3.Max = value;
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
                        string text = "Bottom值不能大于Top值";
                        if (InperConfig.Instance.Language == "en_us")
                        {
                            text = "The Bottom value cannot be greater than the Top value";
                        }
                        Growl.Warning(new GrowlInfo() { Message = text, Token = "SuccessMsg", WaitTime = 1 });
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
                                channel.YVisibleRange1.Min = value;
                                channel.YVisibleRange2.Min = value;
                                channel.YVisibleRange3.Min = value;
                            }
                            CameraSignalSettings.AllChannelConfig.YBottom = value;
                        }
                        else
                        {
                            CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type).YBottom = value;

                            foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                            {
                                if (channel.ChannelId == item.ChannelId && channel.Type == item.Type)
                                {
                                    channel.YVisibleRange.Min = value;
                                    channel.YVisibleRange1.Min = value;
                                    channel.YVisibleRange2.Min = value;
                                    channel.YVisibleRange3.Min = value;
                                }
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
        public void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var item = this.view.selectChannel.SelectedItem as Channel;

                var textbox = sender as System.Windows.Controls.TextBox;
                if (!textbox.IsFocused) return;
                double value = double.Parse(textbox.Text);

                if (textbox.Name.Contains("rangeTop"))
                {
                    //if (value <= item.YBottom)
                    //{
                    //    textbox.Text = (item.YBottom + 0.01).ToString();
                    //    Growl.Warning(new GrowlInfo() { Message = "Top值不能小于Bottom值", Token = "SuccessMsg", WaitTime = 1 });
                    //}
                    //else
                    //{
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
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type).YTop = value;
                        foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                        {
                            if (channel.ChannelId == item.ChannelId && channel.Type == item.Type)
                            {
                                channel.YVisibleRange.Max = value;
                            }
                        }
                    }
                    //}
                }
                else
                {
                    //if (value >= item.YTop)
                    //{
                    //    textbox.Text = (item.YTop - 0.01).ToString();
                    //    Growl.Warning(new GrowlInfo() { Message = "Bottom值不能大于Top值", Token = "SuccessMsg", WaitTime = 1 });
                    //}
                    //else
                    //{
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
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId && x.Type == item.Type).YBottom = value;

                        foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                        {
                            if (channel.ChannelId == item.ChannelId && channel.Type == item.Type)
                            {
                                channel.YVisibleRange.Min = value;
                            }
                        }
                    }
                }
                //}
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Smooth_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var chb = sender as CheckBox;
                var item = this.view.selectChannel.SelectedItem as Channel;
                if (item.ChannelId == _ChannleId)
                {
                    CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        if (chb.Name.Equals("smooth"))
                        {
                            x.Filters.IsSmooth = true;
                        }
                        else if (chb.Name.Equals("bandpass"))
                        {
                            x.Filters.IsBandpass = true;
                        }
                        else if (chb.Name.Equals("bandstop"))
                        {
                            x.Filters.IsBandstop = true;
                        }

                    });
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        view.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (chb.Name.Equals("smooth"))
                            {
                                chn.Filters.IsSmooth = true;
                            }
                            else if (chb.Name.Equals("bandpass"))
                            {
                                chn.Filters.IsBandpass = true;
                            }
                            else if (chb.Name.Equals("bandstop"))
                            {
                                chn.Filters.IsBandstop = true;
                            }
                        }));
                    });
                    //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                    //{
                    //    view.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        if (chb.Name.Equals("smooth"))
                    //        {
                    //            chn.Filters.IsSmooth = true;
                    //        }
                    //        else if (chb.Name.Equals("bandpass"))
                    //        {
                    //            chn.Filters.IsBandpass = true;
                    //        }
                    //        else if (chb.Name.Equals("bandstop"))
                    //        {
                    //            chn.Filters.IsBandstop = true;
                    //        }
                    //    }));
                    //});
                    if (chb.Name.Equals("smooth"))
                    {
                        CameraSignalSettings.AllChannelConfig.Filters.IsSmooth = true;
                    }
                    else if (chb.Name.Equals("bandpass"))
                    {
                        CameraSignalSettings.AllChannelConfig.Filters.IsBandpass = true;
                    }
                    else if (chb.Name.Equals("bandstop"))
                    {
                        CameraSignalSettings.AllChannelConfig.Filters.IsBandstop = true;
                    }

                }
                else
                {
                    if (chb.Name.Equals("smooth"))
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.IsSmooth = true;
                    }
                    else if (chb.Name.Equals("bandpass"))
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.IsBandpass = true;
                    }
                    else if (chb.Name.Equals("bandstop"))
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.IsBandstop = true;
                    }

                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        view.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (chn.ChannelId == item.ChannelId)
                            {
                                if (chb.Name.Equals("smooth"))
                                {
                                    chn.Filters.IsSmooth = true;
                                }
                                else if (chb.Name.Equals("bandpass"))
                                {
                                    chn.Filters.IsBandpass = true;
                                }
                                else if (chb.Name.Equals("bandstop"))
                                {
                                    chn.Filters.IsBandstop = true;
                                }
                            }
                        }));
                    });
                    //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                    //{
                    //    view.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        if (chn.ChannelId == item.ChannelId)
                    //        {
                    //            if (chb.Name.Equals("smooth"))
                    //            {
                    //                chn.Filters.IsSmooth = true;
                    //            }
                    //            else if (chb.Name.Equals("bandpass"))
                    //            {
                    //                chn.Filters.IsBandpass = true;
                    //            }
                    //            else if (chb.Name.Equals("bandstop"))
                    //            {
                    //                chn.Filters.IsBandstop = true;
                    //            }
                    //        }
                    //    }));
                    //});
                }

            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Smooth_UnChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var chb = sender as CheckBox;
                var item = this.view.selectChannel.SelectedItem as Channel;
                if (item.ChannelId == _ChannleId)
                {
                    CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        if (chb.Name.Equals("smooth"))
                        {
                            x.Filters.IsSmooth = false;
                        }
                        else if (chb.Name.Equals("bandpass"))
                        {
                            x.Filters.IsBandpass = false;
                        }
                        else if (chb.Name.Equals("bandstop"))
                        {
                            x.Filters.IsBandstop = false;
                        }
                    });
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        view.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (chb.Name.Equals("smooth"))
                            {
                                chn.Filters.IsSmooth = false;
                            }
                            else if (chb.Name.Equals("bandpass"))
                            {
                                chn.Filters.IsBandpass = false;
                            }
                            else if (chb.Name.Equals("bandstop"))
                            {
                                chn.Filters.IsBandstop = false;
                            }
                        }));
                    });
                    //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                    //{
                    //    view.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        if (chb.Name.Equals("smooth"))
                    //        {
                    //            chn.Filters.IsSmooth = false;
                    //        }
                    //        else if (chb.Name.Equals("bandpass"))
                    //        {
                    //            chn.Filters.IsBandpass = false;
                    //        }
                    //        else if (chb.Name.Equals("bandstop"))
                    //        {
                    //            chn.Filters.IsBandstop = false;
                    //        }
                    //    }));
                    //});
                    if (chb.Name.Equals("smooth"))
                    {
                        CameraSignalSettings.AllChannelConfig.Filters.IsSmooth = false;
                    }
                    else if (chb.Name.Equals("bandpass"))
                    {
                        CameraSignalSettings.AllChannelConfig.Filters.IsBandpass = false;
                    }
                    else if (chb.Name.Equals("bandstop"))
                    {
                        CameraSignalSettings.AllChannelConfig.Filters.IsBandstop = false;
                    }
                }
                else
                {
                    if (chb.Name.Equals("smooth"))
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.IsSmooth = false;
                    }
                    else if (chb.Name.Equals("bandpass"))
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.IsBandpass = false;
                    }
                    else if (chb.Name.Equals("bandstop"))
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.IsBandstop = false;
                    }
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        view.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            if (chn.ChannelId == item.ChannelId)
                            {
                                if (chb.Name.Equals("smooth"))
                                {
                                    chn.Filters.IsSmooth = false;
                                }
                                else if (chb.Name.Equals("bandpass"))
                                {
                                    chn.Filters.IsBandpass = false;
                                }
                                else if (chb.Name.Equals("bandstop"))
                                {
                                    chn.Filters.IsBandstop = false;
                                }
                            }
                        }));
                    });
                    //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                    //{
                    //    view.Dispatcher.BeginInvoke(new Action(() =>
                    //    {
                    //        if (chn.ChannelId == item.ChannelId)
                    //        {
                    //            if (chb.Name.Equals("smooth"))
                    //            {
                    //                chn.Filters.IsSmooth = false;
                    //            }
                    //            else if (chb.Name.Equals("bandpass"))
                    //            {
                    //                chn.Filters.IsBandpass = false;
                    //            }
                    //            else if (chb.Name.Equals("bandstop"))
                    //            {
                    //                chn.Filters.IsBandstop = false;
                    //            }
                    //        }
                    //    }));
                    //});
                }

            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Bandpass_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((sender as System.Windows.Controls.TextBox).IsFocused)
                {
                    var item = this.view.selectChannel.SelectedItem as Channel;
                    double valueHigh = view.high.InperMaxValue;
                    double valueLow = view.low.InperMinValue;

                    if (!string.IsNullOrEmpty(view.high.Text))
                    {
                        valueHigh = double.Parse(view.high.Text);
                    }
                    if (!string.IsNullOrEmpty(view.low.Text))
                    {
                        valueLow = double.Parse(view.low.Text);
                    }
                    if (item.ChannelId == _ChannleId)
                    {
                        CameraSignalSettings.CameraChannels.ForEach(x =>
                        {
                            x.Filters.Bandpass1 = valueLow;
                            x.Filters.Bandpass2 = valueHigh;
                        });
                        foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                        {
                            channel.Filters.Bandpass1 = valueLow;
                            channel.Filters.Bandpass2 = valueHigh;
                        }
                        //foreach (var channel in InperDeviceHelper.Instance._LoopCannels)
                        //{
                        //    channel.Filters.Bandpass1 = valueLow;
                        //    channel.Filters.Bandpass2 = valueHigh;
                        //    channel.Filters.OnlineFilter.Bandpass(InperGlobalClass.CameraSignalSettings.Sampling, valueLow, valueHigh);
                        //}
                        CameraSignalSettings.AllChannelConfig.Filters.Bandpass1 = valueLow;
                        CameraSignalSettings.AllChannelConfig.Filters.Bandpass2 = valueHigh;

                    }
                    else
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.Bandpass1 = valueLow;
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.Bandpass2 = valueHigh;
                        foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                        {
                            if (channel.ChannelId == item.ChannelId)
                            {
                                channel.Filters.Bandpass1 = valueLow;
                                channel.Filters.Bandpass2 = valueHigh;
                            }
                        }
                        //foreach (var channel in InperDeviceHelper.Instance._LoopCannels)
                        //{
                        //    if (channel.ChannelId == item.ChannelId)
                        //    {
                        //        channel.Filters.Bandpass1 = valueLow;
                        //        channel.Filters.Bandpass2 = valueHigh;
                        //        channel.Filters.OnlineFilter.Bandpass(InperGlobalClass.CameraSignalSettings.Sampling, valueLow, valueHigh);
                        //    }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Bandstop_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((sender as System.Windows.Controls.TextBox).IsFocused)
                {
                    var item = this.view.selectChannel.SelectedItem as Channel;
                    double valueHigh = view.stopHigh.InperMaxValue;
                    double valueLow = view.stopLow.InperMinValue;

                    if (!string.IsNullOrEmpty(view.stopHigh.Text))
                    {
                        valueHigh = double.Parse(view.stopHigh.Text);
                    }
                    if (!string.IsNullOrEmpty(view.stopLow.Text))
                    {
                        valueLow = double.Parse(view.stopLow.Text);
                    }
                    if (item.ChannelId == _ChannleId)
                    {
                        CameraSignalSettings.CameraChannels.ForEach(x =>
                        {
                            x.Filters.Bandstop1 = valueLow;
                            x.Filters.Bandstop2 = valueHigh;
                        });
                        foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                        {
                            channel.Filters.Bandstop1 = valueLow;
                            channel.Filters.Bandstop2 = valueHigh;
                        }
                        //foreach (var channel in InperDeviceHelper.Instance._LoopCannels)
                        //{
                        //    channel.Filters.Bandpass1 = valueLow;
                        //    channel.Filters.Bandpass2 = valueHigh;
                        //    channel.Filters.OnlineFilter.Bandstop(InperGlobalClass.CameraSignalSettings.Sampling, valueLow, valueHigh);
                        //}
                        CameraSignalSettings.AllChannelConfig.Filters.Bandstop1 = valueLow;
                        CameraSignalSettings.AllChannelConfig.Filters.Bandstop2 = valueHigh;

                    }
                    else
                    {
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.Bandstop1 = valueLow;
                        CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Filters.Bandstop2 = valueHigh;
                        foreach (var channel in InperDeviceHelper.Instance.CameraChannels)
                        {
                            if (channel.ChannelId == item.ChannelId)
                            {
                                channel.Filters.Bandstop1 = valueLow;
                                channel.Filters.Bandstop2 = valueHigh;
                            }
                        }
                        //foreach (var channel in InperDeviceHelper.Instance._LoopCannels)
                        //{
                        //    if (channel.ChannelId == item.ChannelId)
                        //    {
                        //        channel.Filters.Bandpass1 = valueLow;
                        //        channel.Filters.Bandpass2 = valueHigh;
                        //        channel.Filters.OnlineFilter.Bandstop(InperGlobalClass.CameraSignalSettings.Sampling, valueLow, valueHigh);
                        //    }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void Smooth_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var item = this.view.selectChannel.SelectedItem as Channel;

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
                                InperDeviceHelper.Instance.FilterData[channel.ChannelId][x.LightType] = new System.Collections.Concurrent.ConcurrentQueue<double>();
                            });
                        }
                    }
                    //foreach (var channel in InperDeviceHelper.Instance._LoopCannels)
                    //{
                    //    channel.Filters.Smooth = value;
                    //    if (InperGlobalClass.IsPreview)
                    //    {
                    //        channel.LightModes.ForEach(x =>
                    //        {
                    //            InperDeviceHelper.Instance.FilterData[channel.ChannelId][x.LightType].Clear();
                    //        });
                    //    }
                    //}
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
                                    InperDeviceHelper.Instance.FilterData[channel.ChannelId][x.LightType] = new System.Collections.Concurrent.ConcurrentQueue<double>();
                                });
                            }
                        }
                    }
                    //foreach (var channel in InperDeviceHelper.Instance._LoopCannels)
                    //{
                    //    if (channel.ChannelId == item.ChannelId)
                    //    {
                    //        channel.Filters.Smooth = value;
                    //        if (InperGlobalClass.IsPreview)
                    //        {
                    //            channel.LightModes.ForEach(x =>
                    //            {
                    //                InperDeviceHelper.Instance.FilterData[channel.ChannelId][x.LightType].Clear();
                    //            });
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        [Obsolete]
        public void CameraOffset(string type)
        {
            try
            {
                var item = this.view.selectChannel.SelectedItem as Channel;
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
                        //foreach (var channel in InperDeviceHelper.Instance._LoopCannels)
                        //{
                        //    channel.Offset = false;
                        //    if (InperGlobalClass.IsPreview)
                        //    {
                        //        channel.LightModes.ForEach(x =>
                        //        {
                        //            InperDeviceHelper.Instance.OffsetData[channel.ChannelId][x.LightType].Clear();
                        //            x.OffsetValue = 0;
                        //        });
                        //    }
                        //}
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
                    //view.offset.IsEnabled = true;
                    //view.cancle.IsEnabled = false;
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
                    //view.cancle.IsEnabled = true;
                    //view.offset.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void DuplicateOffset()
        {
            try
            {
                int windowSize = int.Parse(view._windowSize.Text);

                //设置
                var item = this.view.selectChannel.SelectedItem as Channel;
                // 清空内部字典中的队列
                foreach (var outerKeyValuePair in InperDeviceHelper.Instance.OffsetData)
                {
                    if (outerKeyValuePair.Key == item.ChannelId || item.ChannelId == _ChannleId)
                    {
                        foreach (var innerKeyValuePair in outerKeyValuePair.Value)
                        {
                            innerKeyValuePair.Value.Clear();
                        }
                    }
                }
                if (item.ChannelId == _ChannleId)
                {
                    CameraSignalSettings.CameraChannels.ForEach(x =>
                    {
                        x.Offset = true;
                        x.OffsetWindowSize = windowSize;
                    });
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        chn.Offset = true;
                        chn.OffsetWindowSize = windowSize;
                    });
                    //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                    //{
                    //    chn.Offset = true;
                    //    chn.OffsetWindowSize = windowSize;
                    //});
                    CameraSignalSettings.AllChannelConfig.Offset = true;
                    CameraSignalSettings.AllChannelConfig.OffsetWindowSize = windowSize;
                }
                else
                {
                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Offset = true;
                    CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).OffsetWindowSize = windowSize;
                    _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                    {
                        if (chn.ChannelId == item.ChannelId)
                        {
                            chn.Offset = true;
                            chn.OffsetWindowSize = windowSize;
                        }
                    });
                    //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                    //{
                    //    if (chn.ChannelId == item.ChannelId)
                    //    {
                    //        chn.Offset = true;
                    //        chn.OffsetWindowSize = windowSize;
                    //    }
                    //});
                }
                item.Offset = true;
                item.OffsetWindowSize = windowSize;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void InperTextBox_InperTextChanged(object arg1, TextChangedEventArgs arg2)
        {
            try
            {
                InperTextBox textBox = arg1 as InperTextBox;
                var adFrame = uint.Parse(textBox.Text);
                InperDeviceHelper.Instance.AiConfigSend();
                InperDeviceHelper.Instance.adFsTimeInterval = 1d / adFrame;
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void OffsetChecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var cb = sender as CheckBox;
                if (!cb.IsFocused) return;

                //// 清空内部字典中的队列
                //foreach (var outerKeyValuePair in InperDeviceHelper.Instance.OffsetData)
                //{
                //    foreach (var innerKeyValuePair in outerKeyValuePair.Value)
                //    {
                //        innerKeyValuePair.Value.Clear();
                //    }
                //}
                int windowSize = int.Parse(view._windowSize.Text);
                var chb = sender as CheckBox;
                var item = this.view.selectChannel.SelectedItem as Channel;

                CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Offset = true;
                //CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).OffsetWindowSize = windowSize;
                //CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).OffsetValue = item.OffsetValue;

                _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                {
                    view.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (chn.ChannelId == item.ChannelId)
                        {
                            chn.Offset = true;
                            //chn.OffsetWindowSize = windowSize;
                            //chn.OffsetValue = item.OffsetValue;
                        }
                    }));
                });
                //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                //{
                //    view.Dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        if (chn.ChannelId == item.ChannelId)
                //        {
                //            chn.Offset = true;
                //            //chn.OffsetWindowSize = windowSize;
                //            //chn.OffsetValue = item.OffsetValue;
                //        }
                //    }));
                //});
                OffsetValue_Refresh();
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void OffsetUnchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var chb = sender as CheckBox;
                var item = this.view.selectChannel.SelectedItem as Channel;

                CameraSignalSettings.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId).Offset = false;
                //InperDeviceHelper.Instance._LoopCannels.ForEachDo(x =>
                //{
                //    if (x.ChannelId == item.ChannelId)
                //    {
                //        x.Offset = false;
                //    }
                //});
                _ = Parallel.ForEach(InperDeviceHelper.Instance.CameraChannels, chn =>
                {
                    view.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (chn.ChannelId == item.ChannelId)
                        {
                            chn.Offset = false;
                            //chn.
                        }
                    }));
                });
                _ = Parallel.ForEach(Lights, l =>
                {
                    view.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (l.ChannelId == item.ChannelId)
                        {
                            l.OffsetValue = 0.ToString();
                            //chn.
                        }
                    }));
                });
                //_ = Parallel.ForEach(InperDeviceHelper.Instance._LoopCannels, chn =>
                //{
                //    view.Dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        if (chn.ChannelId == item.ChannelId)
                //        {
                //            chn.Offset = false;
                //        }
                //    }));
                //});
            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        public void OffsetValue_Refresh()
        {
            try
            {
                var item = this.view.selectChannel.SelectedItem as Channel;
                if (InperGlobalClass.IsStop)
                {
                    view.refresh_remainder.Visibility = Visibility.Visible;
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Task.Run(async () =>
                    {
                        while (stopwatch.Elapsed.TotalSeconds < 2)
                        {
                            await Task.Delay(10);
                        }
                        stopwatch.Stop();
                        this.view.Dispatcher.Invoke(() =>
                        {
                            view.refresh_remainder.Visibility = Visibility.Collapsed;
                        });
                    });
                }

                if (InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId) is var chn)
                {
                    int size = int.Parse(view._windowSize.Text);
                    if (size >= 0)
                    {
                        chn.OffsetWindowSize = item.OffsetWindowSize = size;
                        if (chn.Type == ChannelTypeEnum.Camera.ToString())
                        {
                            chn.LightModes.ForEach(l =>
                            {
                                if (l.XyDataSeries.YValues.Count() == 0)
                                {
                                    l.OffsetValue = 0;
                                }
                                else
                                {
                                    if (InperDeviceHelper.Instance.OffsetData.ContainsKey(item.ChannelId))
                                    {
                                        if (InperDeviceHelper.Instance.OffsetData[item.ChannelId].ContainsKey(l.LightType))
                                        {
                                            if (InperDeviceHelper.Instance.OffsetData[item.ChannelId][l.LightType].Count > 0)
                                            {
                                                l.OffsetValue = item.OffsetWindowSize == 0 ? 0 : InperDeviceHelper.Instance.OffsetData[item.ChannelId][l.LightType].Count < item.OffsetWindowSize ? InperDeviceHelper.Instance.OffsetData[item.ChannelId][l.LightType].Average() : InperDeviceHelper.Instance.OffsetData[item.ChannelId][l.LightType].Skip(InperDeviceHelper.Instance.OffsetData[item.ChannelId][l.LightType].Count - item.OffsetWindowSize).Average();
                                            }
                                            else
                                            {
                                                l.OffsetValue = item.OffsetWindowSize == 0 ? 0 : l.XyDataSeries.YValues.Count() < item.OffsetWindowSize ? (l.XyDataSeries.YValues.Average() + l.OffsetValue) : (l.XyDataSeries.YValues.Skip(l.XyDataSeries.Count - item.OffsetWindowSize).Average() + l.OffsetValue);
                                            }
                                        }
                                    }
                                }

                                if (InperGlobalClass.IsStop)
                                {
                                    l.OffsetValue = 0;
                                }
                                if (Lights.First(x => x.LightId == l.LightType) is var light)
                                {
                                    double value = Math.Round(l.OffsetValue, 3);
                                    light.OffsetValue = value.ToString();
                                    if (InperGlobalClass.IsStop)
                                    {
                                        light.OffsetValue = 0.ToString();
                                    }
                                }
                            });
                        }
                        else
                        {
                            //double[] array = new double[chn.RenderableSeries.First().DataSeries.Count];
                            double[] array = chn.RenderableSeries.First().DataSeries.YValues.Cast<double>().ToArray();
                            if (array.Length > 0)
                            {
                                chn.LightModes.First().OffsetValue = item.OffsetWindowSize == 0 ? 0 : array.Length < item.OffsetWindowSize ? array.Average() + chn.LightModes.FirstOrDefault().OffsetValue : array.Skip(array.Length - item.OffsetWindowSize).Average() + chn.LightModes.FirstOrDefault().OffsetValue;
                                item.OffsetValue = chn.LightModes.First().OffsetValue.ToString();
                                view.offsetValue.Text = item.OffsetValue.ToString();
                            }
                            if (InperGlobalClass.IsStop)
                            {
                                item.OffsetValue = "0";
                                chn.LightModes.First().OffsetValue = 0;
                                view.offsetValue.Text = "0";
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
        public void OffsetValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if ((sender as System.Windows.Controls.TextBox).IsFocused)
                {
                    var item = this.view.selectChannel.SelectedItem as Channel;
                    if (sender is System.Windows.Controls.TextBox tb)
                    {
                        if (item.Type == ChannelTypeEnum.Analog.ToString())
                        {
                            if (double.TryParse(tb.Text.ToString(), out double num))
                            {
                                item.OffsetValue = num.ToString();
                            }
                        }
                        if (InperDeviceHelper.Instance.CameraChannels.FirstOrDefault(x => x.ChannelId == item.ChannelId) is var chn)
                        {
                            chn.LightModes.First().OffsetValue = ConverterString2Double(item.OffsetValue);
                        }
                        CameraSignalSettings.CameraChannels.ForEachDo(ch =>
                        {
                            if (item.ChannelId == ch.ChannelId)
                            {
                                ch.OffsetValue = item.OffsetValue;
                            }
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                InperLogExtentHelper.LogExtent(ex, this.GetType().Name);
            }
        }
        protected override void OnClose()
        {
            //if (Lights.Count > 0)
            //{
            //    CameraSignalSettings.CameraChannels.ForEachDo(chn =>
            //    {
            //        chn.LightOffsetValue = string.Empty;
            //        Lights.ForEachDo(l =>
            //            {
            //                if (l.ChannelId == chn.ChannelId)
            //                {
            //                    chn.LightOffsetValue += l.LightId.ToString() + ',' + l.OffsetValue + ' ';
            //                }
            //            });
            //    });
            //}
            InperJsonHelper.SetCameraSignalSettings(CameraSignalSettings);
        }
    }
}
