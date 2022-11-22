using InperSight.Lib.Bean;
using InperSight.Lib.Chart;
using InperSight.Lib.Chart.Channel;
using InperSight.Lib.Config;
using InperSight.Lib.Helper;
using InperSight.Views;
using SciChart.Charting.Visuals;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InperSight.ViewModels
{
    public class DataShowControlViewModel : Screen
    {
        #region properties
        private IWindowManager windowManager;
        private DataShowControlView view;
        public static string TextFormat = "hh:mm:ss";
        private double visibleValue = 10;
        public List<string> TextLableFormatting { get; set; } = new List<string>();
        private BindableCollection<CameraChannel> cameraChannels;
        public BindableCollection<CameraChannel> ChartDatas { get => cameraChannels; set => SetAndNotify(ref cameraChannels, value); }
        private EventChannel eventChannel = new EventChannel();
        public EventChannel EventChannelChart { get => eventChannel; set => SetAndNotify(ref eventChannel, value); }
        public double VisibleValue
        {
            get => visibleValue;
            set
            {

                SetAndNotify(ref visibleValue, value);
                if (value > 0)
                {
                    foreach (var channel in ChartDatas)
                    {
                        channel.XVisibleRange = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(value));

                        channel.ViewportManager = new ScrollingViewportManager(value);
                    };
                    EventChannelChart.XVisibleRange = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(value));
                    EventChannelChart.ViewportManager = new ScrollingViewportManager(value);
                    view.sciScroll.SelectedRange = new TimeSpanRange(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, (int)value));
                    Instance_StartCollectEvent(true);
                }
            }
        }
        #endregion
        public DataShowControlViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            ChartDatas = InperDeviceHelper.GetInstance().CameraChannels;
            EventChannelChart = InperDeviceHelper.GetInstance().EventChannelChart;
            TextLableFormatting.Add("hh:mm:ss");
            TextLableFormatting.Add("Seconds");
            TextLableFormatting.Add("ms");
            TextLableFormatting.Add("Time of day");

        }
        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
            view = this.View as DataShowControlView;
            this.view.dataList.PreviewMouseWheel += (s, e) =>
            {
                var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                {
                    RoutedEvent = UIElement.MouseWheelEvent,
                    Source = s
                };
                this.view.dataList.RaiseEvent(eventArg);
            };
            if (EventChannelChart.TimeSpanAxis == null)
            {
                EventChannelChart.TimeSpanAxis = new SciChart.Charting.Visuals.Axes.TimeSpanAxis()
                {
                    DrawMajorBands = false,
                    DrawMajorGridLines = false,
                    DrawMinorGridLines = false,
                    FontSize = 10,
                    VisibleRange = new TimeSpanRange(new TimeSpan(0), new TimeSpan(0, 0, (int)VisibleValue))
                };
            }
            view.timesAxisSci.XAxis.LabelProvider = new CustomTimeSpanLableProvider();
            InperDeviceHelper.GetInstance().StartCollectEvent += Instance_StartCollectEvent;
        }
        private void Instance_StartCollectEvent(bool obj)
        {
            if (obj)
            {
                view.sciScroll.SelectedRange = new TimeSpanRange(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, (int)VisibleValue));
                EventChannelChart.TimeSpanAxis.VisibleRange = new TimeSpanRange(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, (int)VisibleValue));
                foreach (var item in ChartDatas)
                {
                    item.TimeSpanAxis.VisibleRange = new TimeSpanRange(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, (int)VisibleValue));
                }
            }
        }
        #region chart
        public void YaxisAdd(object sender)
        {
            try
            {
                CameraChannel channel = sender as CameraChannel;
                double value = (double)channel.YVisibleRange.Max - 0.1;
                double minValue = (double)channel.YVisibleRange.Min + 0.1;
                if (minValue >= value)
                {
                    //Growl.Info("Reached the limit", "SuccessMsg");
                    return;
                }

                channel.YVisibleRange.Min = Math.Round(minValue, 2);
                channel.YVisibleRange.Max = Math.Round(value, 2);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
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
                    //Growl.Warning("Reached the limit", "SuccessMsg");
                    return;
                }

                channel.YVisibleRange.Min = Math.Round(value, 2);
                channel.YVisibleRange.Max = Math.Round(maxValue, 2);
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
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
                LoggerHelper.Error(ex.ToString());
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
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void SciScroll_SelectedRangeChanged(object sender, SciChart.Charting.Visuals.Events.SelectedRangeChangedEventArgs e)
        {
            try
            {
                if (e.EventType != SciChart.Charting.Visuals.Events.SelectedRangeEventType.ExternalSource)
                {
                    for (int i = 0; i < InperDeviceHelper.GetInstance().CameraChannels.Count; i++)
                    {
                        if (i != 0)
                        {
                            var item = this.view.dataList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                            SciChartSurface sciChartSurface = InperGlobalFunc.FindVisualChild<SciChartSurface>(item);
                            sciChartSurface.ZoomState = ZoomStates.UserZooming;

                            sciChartSurface.XAxis.VisibleRange = e.SelectedRange;
                        }
                    }
                }
                InperDeviceHelper.GetInstance().EventChannelChart.TimeSpanAxis.VisibleRange = e.SelectedRange;
                //InperDeviceHelper.GetInstance().EventChannelChart.EventTimeSpanAxis.VisibleRange = e.SelectedRange;

            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void SciScroll_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                ChartZoomExtents();
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        private void ChartZoomExtents()
        {
            for (int i = 0; i < ChartDatas.Count; i++)
            {
                var item = this.view.dataList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                SciChartSurface sciChartSurface = InperGlobalFunc.FindVisualChild<SciChartSurface>(item);
                sciChartSurface.ZoomState = ZoomStates.AtExtents;
                sciChartSurface.AnimateZoomExtents(TimeSpan.FromMilliseconds(200));
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
                LoggerHelper.Error(ex.ToString());
            }
        }
        public void SciScrollSet()
        {
            try
            {
                view.sciScroll.Axis = InperDeviceHelper.GetInstance().CameraChannels.First().TimeSpanAxis;
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
            }
        }
        #endregion
    }
}
