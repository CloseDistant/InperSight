using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InperStudio.Lib.Bean.Channel
{
    public abstract class ChannelBase : PropertyChangedBase
    {
        public int ChannelId { get; set; }
        public string Name { get; set; }
    }

    public class CameraChannel : ChannelBase
    {
        public Mat Mask { get; set; }
        public double ROI { get; set; }
        private BindableCollection<IRenderableSeriesViewModel> renderableSeriesViewModels = new BindableCollection<IRenderableSeriesViewModel>();
        public BindableCollection<IRenderableSeriesViewModel> RenderableSeries { get => renderableSeriesViewModels; set => SetAndNotify(ref renderableSeriesViewModels, value); }
        public List<LightMode<TimeSpan, double>> LightModes { get; set; } = new List<LightMode<TimeSpan, double>>();
        public ScrollingViewportManager ViewportManager { get; set; } = new ScrollingViewportManager(100000000);
        private IRange xrange = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
        public IRange XVisibleRange { get => xrange; set => SetAndNotify(ref xrange, value); }
        private IRange yrange = new DoubleRange(0, 10);
        public IRange YVisibleRange { get => yrange; set => SetAndNotify(ref yrange, value); }
    }
    public class LightMode<TX, TY> : PropertyChangedBase
        where TX : IComparable
        where TY : IComparable
    {
        /// <summary>
        /// 0 1 2 3
        /// </summary>
        public int LightType { get; set; }
        private XyDataSeries<TX, TY> dataSeries = new XyDataSeries<TX, TY>();
        public XyDataSeries<TX, TY> XyDataSeries { get => dataSeries; set => SetAndNotify(ref dataSeries, value); }
        public SolidColorBrush WaveColor { get; set; }
    }
}
