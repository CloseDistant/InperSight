using HandyControl.Controls;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Chart;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using SciChart.Charting.Visuals;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace InperStudio.ViewModels
{
    public class DataShowControlViewModel : Screen
    {
        #region properties
        private IWindowManager windowManager;
        private DataShowControlView view;
        private BindableCollection<CameraChannel> cameraChannels;
        public BindableCollection<CameraChannel> ChartDatas { get => cameraChannels; set => SetAndNotify(ref cameraChannels, value); }
        private EventChannelChart eventChannel;
        public EventChannelChart EventChannelChart { get => eventChannel; set => SetAndNotify(ref eventChannel, value); }
        public List<string> TextLableFormatting { get; set; } = new List<string>();
        public static string TextFormat = "hh:mm:ss";
        private double visibleValue = 10;

        public double VisibleValue
        {
            get => visibleValue;
            set
            {
                SetAndNotify(ref visibleValue, value);
                foreach (var channel in ChartDatas)
                {
                    channel.XVisibleRange = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(value));

                    channel.ViewportManager = new ScrollingViewportManager(value);
                };
                EventChannelChart.XVisibleRange = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(value));
                EventChannelChart.ViewportManager = new ScrollingViewportManager(value);
            }
        }
        #endregion
        public DataShowControlViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            ChartDatas = InperDeviceHelper.Instance.CameraChannels;
            EventChannelChart = InperDeviceHelper.Instance.EventChannelChart;
            TextLableFormatting.Add("hh:mm:ss");
            TextLableFormatting.Add("Seconds");
            TextLableFormatting.Add("ms");
            TextLableFormatting.Add("Time of day");
            InperGlobalClass.IsExistEvent = true;
        }
        private void Instance_StartCollectEvent(bool obj)
        {
            if (obj)
            {
                view.sciScroll.SelectedRange = new TimeSpanRange(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, (int)VisibleValue));
            }
        }
        protected override void OnViewLoaded()
        {
            try
            {
                view = View as DataShowControlView;
                InperGlobalClass.IsExistEvent = false;

                this.view.dataList.PreviewMouseWheel += (s, e) =>
                {
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = s
                    };
                    this.view.dataList.RaiseEvent(eventArg);
                };
                if (EventChannelChart.TimeSpanAxis != null)
                {
                    VisibleValue = ((TimeSpan)EventChannelChart.TimeSpanAxis.VisibleRange.Diff).TotalSeconds;
                }

                if (InperGlobalClass.EventPanelProperties.HeightAuto && this.view.dataList.Items.Count > 0)
                {
                    this.view.relativeBottom.Height = this.view.fixedBottom.Height = this.view.dataList.ActualHeight / this.view.dataList.Items.Count;
                }
                else
                {
                    this.view.relativeBottom.Height = this.view.fixedBottom.Height = InperGlobalClass.EventPanelProperties.HeightFixedValue;
                }
                InperDeviceHelper.Instance.StartCollectEvent += Instance_StartCollectEvent;

                view.timesAxisSci.XAxis.LabelProvider = new CustomTimeSpanLableProvider();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #region methods
        public void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Grid grid = sender as Grid;
                CameraChannel channel = grid.DataContext as CameraChannel;
                if (e.ClickCount == 2)
                {
                    if (grid.Name.Equals("chartItem"))
                    {
                        this.windowManager.ShowDialog(new SignalPropertiesViewModel(SignalPropertiesTypeEnum.Camera, channel.ChannelId));
                    }
                    else
                    {
                        this.windowManager.ShowDialog(new EventPanelPropertiesViewModel(this.view));
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void NumericAxis_VisibleRangeChanged(object sender, SciChart.Charting.Visuals.Events.VisibleRangeChangedEventArgs e)
        {
            try
            {
                SciChart.Charting.Visuals.Axes.NumericAxis axis = sender as SciChart.Charting.Visuals.Axes.NumericAxis;
                axis.VisibleRange = new DoubleRange(-1, 2);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void SciScrollSet()
        {
            try
            {
                view.sciScroll.Axis = InperDeviceHelper.Instance.CameraChannels.Last().TimeSpanAxis;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void MarkerMonitor_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                InperGlobalClass.EventSettings.Channels.ForEach(x =>
                {
                    if (x.IsActive && x.Type == ChannelTypeEnum.Manual.ToString())
                    {
                        AddMarkers(x, 0);
                    }

                    if (x.IsActive && x.Type == ChannelTypeEnum.Output.ToString() && x.Condition != null)
                    {
                        if (x.Condition.Type == ChannelTypeEnum.Manual.ToString())
                        {
                            AddMarkers(x.Condition, 1);
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        private void AddMarkers(EventChannelJson x, int type = 0)
        {
            string[] hotkeys = x.Hotkeys.Split('+');
            if (x.HotkeysCount == 1)
            {
                if (Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[0])))
                {
                    if (type == 0)
                    {
                        InperDeviceHelper.Instance.AddMarkerByHotkeys(x.ChannelId, x.Name, (Color)ColorConverter.ConvertFromString(x.BgColor));
                    }
                    else
                    {
                        InperDeviceHelper.Instance.SendCommand(x);
                    }
                }
            }
            if (x.HotkeysCount == 2)
            {
                if (Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[0])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[1])))
                {
                    if (type == 0)
                    {
                        InperDeviceHelper.Instance.AddMarkerByHotkeys(x.ChannelId, x.Name, (Color)ColorConverter.ConvertFromString(x.BgColor));
                    }
                    else
                    {
                        InperDeviceHelper.Instance.SendCommand(x);
                    }
                }
            }
            if (x.HotkeysCount == 3)
            {
                if (Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[0])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[1])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[2])))
                {
                    if (type == 0)
                    {
                        InperDeviceHelper.Instance.AddMarkerByHotkeys(x.ChannelId, x.Name, (Color)ColorConverter.ConvertFromString(x.BgColor));
                    }
                    else
                    {
                        InperDeviceHelper.Instance.SendCommand(x);
                    }
                }
            }
            int count = InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues.Count;
            TimeSpan time = (TimeSpan)InperDeviceHelper.Instance.CameraChannels[0].RenderableSeries.First().DataSeries.XValues[count - 1];
            if (type == 0 && InperGlobalClass.IsRecord)
            {

                Manual manual = new Manual()
                {
                    ChannelId = x.ChannelId,
                    Color = x.BgColor,
                    CameraTime = time.Ticks,
                    Name = x.Name,
                    Type = ChannelTypeEnum.Manual.ToString(),
                    CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
                };

                _ = (App.SqlDataInit?.sqlSugar.Insertable(manual).ExecuteCommand());
            }
            if (type == 1 && InperGlobalClass.IsRecord)
            {
                Output output = new Output()
                {
                    ChannelId = x.ChannelId,
                    CameraTime = time.Ticks,
                    CreateTime = DateTime.Parse(DateTime.Now.ToString("G"))
                };
                _ = (App.SqlDataInit?.sqlSugar.Insertable(output).ExecuteCommand());
            }

        }
        public void YaxisAdd(object sender)
        {
            try
            {
                CameraChannel channel = sender as CameraChannel;
                double value = (double)channel.YVisibleRange.Max - 0.1;
                double min = (double)channel.YVisibleRange.Min + 0.1;

                if (min >= value)
                {
                    Growl.Info("Reached the limit", "SuccessMsg");
                    return;
                }

                channel.YVisibleRange.Max = Math.Round(value, 2);
                channel.YVisibleRange.Min = Math.Round(min, 2);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void YaxisReduce(object sender)
        {
            try
            {
                CameraChannel channel = sender as CameraChannel;
                double value = (double)channel.YVisibleRange.Min - 0.5;
                double maxValue = (double)channel.YVisibleRange.Max + 0.5;

                if (maxValue > 100)
                {
                    Growl.Warning("Reached the limit", "SuccessMsg");
                    return;
                }

                channel.YVisibleRange.Min = Math.Round(value, 2);
                channel.YVisibleRange.Max = Math.Round(maxValue, 2);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void YaxisNormal(object sender, RoutedEventArgs e)
        {
            try
            {
                Button but = sender as Button;

                SciChartSurface sci = (SciChartSurface)but.FindName("sciChartSurface");

                sci.ZoomExtentsY();
                //CameraChannel channel = sender as CameraChannel;

                //_ = channel.YVisibleRange.SetMinMaxWithLimit(0, 100, new DoubleRange(0, 10));

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void TextFormat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string item = (sender as System.Windows.Controls.ComboBox).SelectedValue.ToString();
                if (!string.IsNullOrEmpty(item))
                {
                    TextFormat = item;
                    if (view != null)
                    {
                        view.timesAxisSci.XAxis.TextFormatting = TextFormat;
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void TextFormatChanged(string type)
        {
            try
            {
                if (type.Equals("Add"))
                {
                    VisibleValue++;
                }
                if (type.Equals("Reduce"))
                {
                    VisibleValue--;
                }
                //this.Refresh();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion
    }
}
