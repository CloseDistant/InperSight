using HandyControl.Controls;
using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Enum;
using InperStudio.Lib.Helper;
using InperStudio.Views;
using SciChart.Charting.Visuals;
using SciChart.Data.Model;
using Stylet;
using System;
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
        public EventChannelChart EventChannelChart { get; set; }
        #endregion
        public DataShowControlViewModel(IWindowManager windowManager)
        {
            this.windowManager = windowManager;
            ChartDatas = InperDeviceHelper.Instance.CameraChannels;
            EventChannelChart = InperDeviceHelper.Instance.EventChannelChart;
        }
        protected override void OnViewLoaded()
        {
            try
            {
                view = this.View as DataShowControlView;

                view.sciScroll.SelectedRangeChanged += (s, e) =>
                {
                    Parallel.ForEach(ChartDatas, item =>
                    {
                        item.XVisibleRange = e.SelectedRange;
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
                if (e.ClickCount == 2)
                {
                    this.windowManager.ShowDialog(new EventPanelPropertiesViewModel(this.view));
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
                    if (x.IsActive && x.Type == EventSettingsTypeEnum.Marker.ToString())
                    {
                        string[] hotkeys = x.Hotkeys.Split('+');
                        if (x.HotkeysCount == 1)
                        {
                            if (Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[0])))
                            {
                                InperDeviceHelper.Instance.AddMarkerByHotkeys(x.Name, (Color)ColorConverter.ConvertFromString(x.BgColor));
                            }
                        }
                        if (x.HotkeysCount == 2)
                        {
                            if (Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[0])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[1])))
                            {
                                InperDeviceHelper.Instance.AddMarkerByHotkeys(x.Name, (Color)ColorConverter.ConvertFromString(x.BgColor));
                            }
                        }
                        if (x.HotkeysCount == 3)
                        {
                            if (Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[0])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[1])) && Keyboard.IsKeyDown((Key)Enum.Parse(typeof(Key), hotkeys[2])))
                            {
                                InperDeviceHelper.Instance.AddMarkerByHotkeys(x.Name, (Color)ColorConverter.ConvertFromString(x.BgColor));
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
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
        public void EventYaxisAdd(object sender)
        {
            try
            {
                EventChannelChart channel = sender as EventChannelChart;
                

                
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
                double value = ((double)channel.YVisibleRange.Min - 1) < 0 ? 0 : ((double)channel.YVisibleRange.Min - 1);

                channel.YVisibleRange.Min = Math.Round(value, 2);

                if (value <= 0.01)
                {
                    Growl.Info("已达到最小值", "SuccessMsg");
                }
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void EventYaxisReduce(object sender)
        {
            try
            {
                EventChannelChart channel = sender as EventChannelChart;
                
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

                //channel.YVisibleRange.Min = 0;
                //channel.YVisibleRange.Max = 10;
                channel.YVisibleRange.SetMinMaxWithLimit(0, 100, new DoubleRange(0, 10));
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        public void EventYaxisNormal(object sender)
        {
            try
            {
                EventChannelChart channel = sender as EventChannelChart;

                
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
        }
        #endregion
    }
}
