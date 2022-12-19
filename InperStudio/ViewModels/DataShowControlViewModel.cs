using GongSolutions.Wpf.DragDrop;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Chart;
using InperStudio.Lib.Data.Model;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InperStudio.ViewModels
{
    class HotKeysListen
    {
        public int Timestamp { get; set; }
        public Key Key { get; set; }
        public bool IsPressed { get; set; } = true;
    }
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
        private ConcurrentQueue<HotKeysListen> _hotKeysListens = new ConcurrentQueue<HotKeysListen>();
        public static double ShowVisibleValue = 10;
        private double visibleValue = 10;
        public double VisibleValue
        {
            get => visibleValue;
            set
            {
                ShowVisibleValue = value;
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
                EventChannelChart.TimeSpanAxis.VisibleRange = new TimeSpanRange(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, (int)VisibleValue));
                foreach (var item in InperDeviceHelper.Instance.CameraChannels)
                {
                    item.TimeSpanAxis.VisibleRange = new TimeSpanRange(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, (int)VisibleValue));
                }
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
        private void ChartZoomExtents()
        {
            for (int i = 0; i < InperDeviceHelper.Instance.CameraChannels.Count; i++)
            {
                var item = this.view.dataList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                SciChartSurface sciChartSurface = InperClassHelper.FindVisualChild<SciChartSurface>(item);
                if (sciChartSurface != null)
                {
                    sciChartSurface.ZoomState = ZoomStates.AtExtents;
                    sciChartSurface.AnimateZoomExtents(TimeSpan.FromMilliseconds(200));
                }
            }
        }
        public void ChartZoomExtentsExport()
        {
            ChartZoomExtents();
        }
        public void SciScroll_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                ChartZoomExtents();
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void SciScroll_SelectedRangeChanged(object sender, SciChart.Charting.Visuals.Events.SelectedRangeChangedEventArgs e)
        {
            try
            {
                if (e.EventType != SciChart.Charting.Visuals.Events.SelectedRangeEventType.ExternalSource)
                {
                    for (int i = 0; i < InperDeviceHelper.Instance.CameraChannels.Count; i++)
                    {
                        if (i != 0)
                        {
                            var item = this.view.dataList.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                            SciChartSurface sciChartSurface = InperClassHelper.FindVisualChild<SciChartSurface>(item);
                            sciChartSurface.ZoomState = ZoomStates.UserZooming;

                            sciChartSurface.XAxis.VisibleRange = e.SelectedRange;
                        }
                    }
                }
                InperDeviceHelper.Instance.EventChannelChart.TimeSpanAxis.VisibleRange = e.SelectedRange;
                InperDeviceHelper.Instance.EventChannelChart.EventTimeSpanAxis.VisibleRange = e.SelectedRange;
                InperDeviceHelper.Instance.EventChannelChart.EventTimeSpanAxisFixed.VisibleRange = e.SelectedRange;

            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
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
                        if (channel.Type == ChannelTypeEnum.Camera.ToString())
                        {
                            this.windowManager.ShowDialog(new SignalPropertiesViewModel(SignalPropertiesTypeEnum.Camera, channel.ChannelId));
                        }
                        else
                        {
                            this.windowManager.ShowDialog(new SignalPropertiesViewModel(SignalPropertiesTypeEnum.Analog, channel.ChannelId));
                        }
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
                view.sciScroll.Axis = InperDeviceHelper.Instance.CameraChannels.First().TimeSpanAxis;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #region 合并拆分
        public void YaxisSet(object sender)
        {
            try
            {
                CameraChannel channel = sender as CameraChannel;
                if (!channel.YaxisCountToZero)
                {
                    channel.S0Visible = true;
                    channel.S1Visible = false;
                    channel.S2Visible = false;
                    channel.S3Visible = false;
                    channel.RenderableSeries.ToList().ForEach(line =>
                    {
                        line.YAxisId = "Ch0";
                    });
                }
                else
                {
                    channel.S0Visible = false;
                    channel.S1Visible = false;
                    channel.S2Visible = false;
                    channel.S3Visible = false;
                    channel.RenderableSeries.ToList().ForEach(line =>
                    {
                        int groupId = int.Parse((line as LineRenderableSeriesViewModel).Tag.ToString());
                        switch (groupId)
                        {
                            case 0:
                                channel.S0Visible = true;
                                break;
                            case 1:
                                channel.S1Visible = true;
                                break;
                            case 2:
                                channel.S2Visible = true;
                                break;
                            case 3:
                                channel.S3Visible = true;
                                break;

                        }
                        line.YAxisId = "Ch" + (line as LineRenderableSeriesViewModel).Tag.ToString();
                    });
                }
                channel.YaxisCountToZero = !channel.YaxisCountToZero;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void AllYaxisSetMerge()
        {
            InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(x =>
            {
                if (x.Type == ChannelTypeEnum.Camera.ToString())
                {
                    x.YaxisCountToZero = true;
                    x.S0Visible = true;
                    x.S1Visible = false;
                    x.S2Visible = false;
                    x.S3Visible = false;
                    x.RenderableSeries.ToList().ForEach(line =>
                    {
                        line.YAxisId = "Ch0";
                    });
                }
            });
        }
        public void AllYaxisSetSeparate()
        {
            InperDeviceHelper.Instance.CameraChannels.ToList().ForEach(x =>
            {
                if (x.Type == ChannelTypeEnum.Camera.ToString())
                {
                    x.S0Visible = false;
                    x.S1Visible = false;
                    x.S2Visible = false;
                    x.S3Visible = false;
                    x.YaxisCountToZero = false;

                    x.RenderableSeries.ToList().ForEach(line =>
                    {
                        int groupId = int.Parse((line as LineRenderableSeriesViewModel).Tag.ToString());
                        switch (groupId)
                        {
                            case 0:
                                x.S0Visible = true;
                                break;
                            case 1:
                                x.S1Visible = true;
                                break;
                            case 2:
                                x.S2Visible = true;
                                break;
                            case 3:
                                x.S3Visible = true;
                                break;

                        }
                        line.YAxisId = "Ch" + (line as LineRenderableSeriesViewModel).Tag.ToString();
                    });
                }
            });
        }
        #endregion
        public void YaxisAdd(object sender)
        {
            try
            {
                CameraChannel channel = sender as CameraChannel;
                double value = (double)channel.YVisibleRange.Max - 0.1;
                double minValue = (double)channel.YVisibleRange.Min + 0.1;
                double value1 = (double)channel.YVisibleRange1.Min + 0.1;
                double minValue1 = (double)channel.YVisibleRange1.Max - 0.1;
                double value2 = (double)channel.YVisibleRange2.Min + 0.1;
                double minValue2 = (double)channel.YVisibleRange2.Max - 0.1;
                double value3 = (double)channel.YVisibleRange3.Min + 0.1;
                double minValue3 = (double)channel.YVisibleRange3.Max - 0.1;
                if (minValue >= value)
                {
                    //Growl.Info("Reached the limit", "SuccessMsg");
                    return;
                }

                channel.YVisibleRange.Min = Math.Round(minValue, 2);
                channel.YVisibleRange.Max = Math.Round(value, 2);
                channel.YVisibleRange1.Min = Math.Round(value1, 2);
                channel.YVisibleRange1.Max = Math.Round(minValue1, 2);
                channel.YVisibleRange2.Min = Math.Round(value2, 2);
                channel.YVisibleRange2.Max = Math.Round(minValue2, 2);
                channel.YVisibleRange3.Min = Math.Round(value3, 2);
                channel.YVisibleRange3.Max = Math.Round(minValue3, 2);
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
                double value1 = (double)channel.YVisibleRange1.Min - 0.5;
                double maxValue1 = (double)channel.YVisibleRange1.Max + 0.5;
                double value2 = (double)channel.YVisibleRange2.Min - 0.5;
                double maxValue2 = (double)channel.YVisibleRange2.Max + 0.5;
                double value3 = (double)channel.YVisibleRange3.Min - 0.5;
                double maxValue3 = (double)channel.YVisibleRange3.Max + 0.5;
                if (maxValue > 100)
                {
                    //Growl.Warning("Reached the limit", "SuccessMsg");
                    return;
                }

                channel.YVisibleRange.Min = Math.Round(value, 2);
                channel.YVisibleRange.Max = Math.Round(maxValue, 2);
                channel.YVisibleRange1.Min = Math.Round(value1, 2);
                channel.YVisibleRange1.Max = Math.Round(maxValue1, 2);
                channel.YVisibleRange2.Min = Math.Round(value2, 2);
                channel.YVisibleRange2.Max = Math.Round(maxValue2, 2);
                channel.YVisibleRange3.Min = Math.Round(value3, 2);
                channel.YVisibleRange3.Max = Math.Round(maxValue3, 2);
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
