using InperStudio.Lib.Helper.JsonBean;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace InperStudio.Lib.Bean.Channel
{
    public class VideoZone
    {
        public string Name { get; set; }
        public List<ZoneConditions> AllZoneConditions { get; set; } = new List<ZoneConditions>();
    }
    public class ZoneConditions
    {
        public string ZoneName { get; set; }
        public bool IsImmediately { get; set; }
        public bool IsDelay { get; set; }
        public int DelaySeconds { get; set; } = 3;
        public int Duration { get; set; } = 3;
        public double ShapeLeft { get; set; }
        public double ShapeTop { get; set; }
        public double ShapeWidth { get; set; }
        public double ShapeHeight { get; set; }
        public string ShapeType { get; set; }
        public string ShapeName { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }

    }
    public class EventChannel : ChannelBase
    {
        private bool isActive = false;
        public bool IsActive { get => isActive; set => SetAndNotify(ref isActive, value); }
        private string bgColor = Brushes.Red.ToString();
        public string BgColor { get => bgColor; set => SetAndNotify(ref bgColor, value); }
        public string Hotkeys { get; set; }
        public double DeltaF { get; set; } = 5;
        private double refractoryPeriod = 3;
        public double RefractoryPeriod { get => refractoryPeriod; set => SetAndNotify(ref refractoryPeriod, value); }
        public int LightIndex { get; set; }
        public int WindowSize { get; set; } = 300;
        public double Tau1 { get; set; }
        public double Tau2 { get; set; }
        public double Tau3 { get; set; }
        public VideoZone VideoZone { get; set; }
        public EventChannelJson Condition { get; set; }
    }
    public class EventChannelChart : PropertyChangedBase
    {
        private TimeSpanAxis timeSpanAxis;
        public TimeSpanAxis TimeSpanAxis { get => timeSpanAxis; set => SetAndNotify(ref timeSpanAxis, value); }
        private TimeSpanAxis eventtimeSpanAxis;
        public TimeSpanAxis EventTimeSpanAxis { get => eventtimeSpanAxis; set => SetAndNotify(ref eventtimeSpanAxis, value); }
        private TimeSpanAxis eventtimeSpanAxisFixed;
        public TimeSpanAxis EventTimeSpanAxisFixed { get => eventtimeSpanAxisFixed; set => SetAndNotify(ref eventtimeSpanAxisFixed, value); }
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
