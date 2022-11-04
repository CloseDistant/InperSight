using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InperSight.Lib.Chart.Channel
{
    public class EventChannel : PropertyChangedBase
    {
        private TimeSpanAxis timeSpanAxis;
        public TimeSpanAxis TimeSpanAxis { get => timeSpanAxis; set => SetAndNotify(ref timeSpanAxis, value); }
        private TimeSpanAxis eventtimeSpanAxis;
        public TimeSpanAxis EventTimeSpanAxis { get => eventtimeSpanAxis; set => SetAndNotify(ref eventtimeSpanAxis, value); }
        private BindableCollection<IAnnotationViewModel> annotations = new BindableCollection<IAnnotationViewModel>();
        public BindableCollection<IAnnotationViewModel> Annotations { get => annotations; set => SetAndNotify(ref annotations, value); }
        private ScrollingViewportManager viewportManager = new ScrollingViewportManager(10);
        public ScrollingViewportManager ViewportManager { get => viewportManager; set => SetAndNotify(ref viewportManager, value); }
        private IRange range = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
        public IRange XVisibleRange { get => range; set => SetAndNotify(ref range, value); }
    }
}
