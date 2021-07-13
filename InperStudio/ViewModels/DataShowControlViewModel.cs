using InperStudio.Lib.Bean;
using InperStudio.Lib.Bean.Channel;
using InperStudio.Lib.Helper;
using InperStudio.Lib.Helper.JsonBean;
using InperStudio.Views;
using SciChart.Charting.Visuals;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
                var item = view.dataList.ItemContainerGenerator.ContainerFromIndex(view.dataList.ItemContainerGenerator.Items.Count - 1) as ListBoxItem;

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
        #endregion
    }
}
