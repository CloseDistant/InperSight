using SciChart.Charting.Model.ChartSeries;
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
    public class EventChannel : ChannelBase
    {
        private bool isActive = false;
        public bool IsActive { get => isActive; set => SetAndNotify(ref isActive, value); }
        public string BgColor { get; set; } = Brushes.Red.ToString();
        public string Hotkeys { get; set; }
        public double DeltaF { get; set; }
    }
    public class EventChannelChart : PropertyChangedBase
    {
        public Dictionary<int, Queue<KeyValuePair<long, double>>> EventQs { get; set; } = new Dictionary<int, Queue<KeyValuePair<long, double>>>();
        public ScrollingViewportManager ViewportManager { get; set; } = new ScrollingViewportManager(100000000);
        private IRange range = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
        public IRange XVisibleRange { get => range; set => SetAndNotify(ref range, value); }

        private BindableCollection<IRenderableSeriesViewModel> renderableSeriesViewModels = new BindableCollection<IRenderableSeriesViewModel>();
        public BindableCollection<IRenderableSeriesViewModel> RenderableSeries { get => renderableSeriesViewModels; set => SetAndNotify(ref renderableSeriesViewModels, value); }
    }
}
