using Newtonsoft.Json;
using OpenCvSharp;
using SciChart.Charting.Model;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Chart.Channel
{
    public abstract class ChannelBase : PropertyChangedBase
    {
        public int ChannelId { get; set; }
        public string SymbolName { get; set; }
        private string name;
        public string Name { get => name; set => SetAndNotify(ref name, value); }
    }
    public class CameraChannel : ChannelBase
    {
        public Mat Mask { get; set; }
        public double ROI { get; set; }
        private double height = double.NaN;
        public double Height { get => height; set => SetAndNotify(ref height, value); }
        private bool autoRange = true;
        public bool AutoRange { get => autoRange; set => SetAndNotify(ref autoRange, value); }
        [JsonIgnore]
        private TimeSpanAxis timeSpanAxis;
        [JsonIgnore]
        public TimeSpanAxis TimeSpanAxis { get => timeSpanAxis; set => SetAndNotify(ref timeSpanAxis, value); }
        [JsonIgnore]
        private BindableCollection<IRenderableSeriesViewModel> renderableSeriesViewModels = new BindableCollection<IRenderableSeriesViewModel>();
        [JsonIgnore]
        public BindableCollection<IRenderableSeriesViewModel> RenderableSeries { get => renderableSeriesViewModels; set => SetAndNotify(ref renderableSeriesViewModels, value); }
        [JsonIgnore]
        private ScrollingViewportManager viewportManager = new ScrollingViewportManager(10);
        [JsonIgnore]
        public ScrollingViewportManager ViewportManager { get => viewportManager; set => SetAndNotify(ref viewportManager, value); }
        [JsonIgnore]
        private IRange xrange = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
        [JsonIgnore]
        public IRange XVisibleRange { get => xrange; set => SetAndNotify(ref xrange, value); }
        [JsonIgnore]
        private IRange yrange = new DoubleRange(0, 10);
        [JsonIgnore]
        public IRange YVisibleRange { get => yrange; set => SetAndNotify(ref yrange, value); }
    }
}
