using System;
using SciChart.Charting.ViewportManagers;
using SciChart.Charting.Visuals;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;

namespace InperStudio.Lib.Bean
{
    public class ScrollingViewportManager : DefaultViewportManager
    {
        private readonly double _windowSize;
        public ScrollingViewportManager(double windowSize)
        {
            _windowSize = windowSize;
        }
        public override void AttachSciChartSurface(ISciChartSurface scs)
        {
            base.AttachSciChartSurface(scs);
            this.ParentSurface = scs;
        }
        public ISciChartSurface ParentSurface { get; private set; }
        protected override IRange OnCalculateNewXRange(IAxis xAxis)
        {
            // The Current XAxis VisibleRange
            var currentVisibleRange = xAxis.VisibleRange as TimeSpanRange;
            if (ParentSurface.ZoomState == ZoomStates.UserZooming)
                return currentVisibleRange;     // Don't scroll if user is zooming
                                                // The MaxXRange is the VisibleRange on the XAxis if we were to zoom to fit all data
            var maxXRange = xAxis.GetMaximumRange() as TimeSpanRange;
            double xMax = Math.Max(maxXRange.Max.TotalSeconds, currentVisibleRange.Max.TotalSeconds);
            // Scroll showing latest window size
            return new TimeSpanRange(TimeSpan.FromSeconds(xMax - _windowSize), TimeSpan.FromSeconds(xMax));
        }

    }
}
