using HandyControl.Controls;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
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
        protected override void OnViewLoaded()
        {
            try
            {
                view = this.View as DataShowControlView;
                InperGlobalClass.IsExistEvent = false;

                view.sciScroll.SelectedRangeChanged += (s, e) =>
                {
                    Parallel.ForEach(ChartDatas, item =>
                    {
                        item.XVisibleRange = e.SelectedRange;
                        EventChannelChart.XVisibleRange = e.SelectedRange;
                    });

                };

                this.view.dataList.PreviewMouseWheel += (s, e) =>
                {
                    var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                    {
                        RoutedEvent = UIElement.MouseWheelEvent,
                        Source = s
                    };
                    this.view.dataList.RaiseEvent(eventArg);
                };

                VisibleValue = ((TimeSpan)this.view.tiemsAxis.VisibleRange.Diff).TotalSeconds;

                if (InperGlobalClass.EventPanelProperties.HeightAuto)
                {
                    this.view.relativeBottom.Height = this.view.fixedBottom.Height = this.view.dataList.ActualHeight / this.view.dataList.Items.Count;
                }
                else
                {
                    this.view.relativeBottom.Height = this.view.fixedBottom.Height = InperGlobalClass.EventPanelProperties.HeightFixedValue;
                }
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
        public void SciScrollSet()
        {
            try
            {
                var item = view.dataList.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;

                ContentPresenter myContentPresenter = InperClassHelper.FindVisualChild<ContentPresenter>(item);

                DataTemplate template = myContentPresenter.ContentTemplate;

                SciChartSurface sciChart = (SciChartSurface)template.FindName("sciChartSurface", myContentPresenter);
                sciChart.XAxis.VisibleRange = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(VisibleValue));
                view.sciScroll.Axis = sciChart.XAxis;
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
                double value = ((double)channel.YVisibleRange.Max + 1) > 100 ? 100 : ((double)channel.YVisibleRange.Max + 1);

                channel.YVisibleRange.Max = Math.Round(value, 2);

                if (value >= 100)
                {
                    Growl.Info("已达到最大值", "SuccessMsg");
                }
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
                double value = (double)channel.YVisibleRange.Min - 1;

                channel.YVisibleRange.Min = Math.Round(value, 2);
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void YaxisNormal(object sender)
        {
            try
            {
                CameraChannel channel = sender as CameraChannel;

                _ = channel.YVisibleRange.SetMinMaxWithLimit(0, 100, new DoubleRange(0, 10));
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
