using InperStudio.Lib.Helper.JsonBean;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals.Annotations;
using SciChart.Charting.Visuals.Axes;
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
        public double DeltaF { get; set; } = 5;
        public int LightIndex { get; set; }
        public int WindowSize { get; set; } = 300;
        public double Tau1 { get; set; }
        public double Tau2 { get; set; }
        public double Tau3 { get; set; }

        public EventChannelJson Condition { get; set; }
    }
    public class EventChannelChart : PropertyChangedBase
    {
        public Dictionary<int, Queue<KeyValuePair<long, double>>> EventQs { get; set; } = new Dictionary<int, Queue<KeyValuePair<long, double>>>();
        private TimeSpanAxis timeSpanAxis;
        public TimeSpanAxis TimeSpanAxis { get => timeSpanAxis; set => SetAndNotify(ref timeSpanAxis, value); }
        private TimeSpanAxis eventtimeSpanAxis;
        public TimeSpanAxis EventTimeSpanAxis { get => eventtimeSpanAxis; set => SetAndNotify(ref eventtimeSpanAxis, value); }
        private TimeSpanAxis eventtimeSpanAxisFixed;
        public TimeSpanAxis EventTimeSpanAxisFixed{ get => eventtimeSpanAxisFixed; set => SetAndNotify(ref eventtimeSpanAxisFixed, value); }
        private BindableCollection<IAnnotationViewModel> annotations = new BindableCollection<IAnnotationViewModel>();
        public BindableCollection<IAnnotationViewModel> Annotations { get => annotations; set => SetAndNotify(ref annotations, value); }
        private ScrollingViewportManager viewportManager = new ScrollingViewportManager(10);
        public ScrollingViewportManager ViewportManager { get => viewportManager; set => SetAndNotify(ref viewportManager, value); }
        private IRange range = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
        public IRange XVisibleRange { get => range; set => SetAndNotify(ref range, value); }

        private BindableCollection<IRenderableSeriesViewModel> renderableSeriesViewModels = new BindableCollection<IRenderableSeriesViewModel>();
        public BindableCollection<IRenderableSeriesViewModel> RenderableSeries { get => renderableSeriesViewModels; set => SetAndNotify(ref renderableSeriesViewModels, value); }
    }
}
