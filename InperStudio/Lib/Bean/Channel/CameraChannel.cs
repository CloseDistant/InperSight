﻿using InperStudio.Lib.Helper.FilterTools;
using OpenCvSharp;
using SciChart.Charting.Model.ChartSeries;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace InperStudio.Lib.Bean.Channel
{
    public abstract class ChannelBase : PropertyChangedBase
    {
        public int ChannelId { get; set; }
        public string SymbolName { get; set; }
        private string name;
        public string Name { get => name; set => SetAndNotify(ref name, value); }
        public string Type { get; set; } = "Camera";
    }

    public class CameraChannel : ChannelBase
    {
        public int CameraPlotCount = 0;
        public Mat Mask { get; set; }
        public double ROI { get; set; }
        private double height = double.NaN;
        public double Height { get => height; set => SetAndNotify(ref height, value); }
        public int OffsetWindowSize { get; set; } = 300;
        public bool Offset { get; set; }
        public bool IsDeltaFCalculate { get; set; }
        private bool autoRange = true;
        public bool AutoRange { get => autoRange; set => SetAndNotify(ref autoRange, value); }
        public Filters Filters { get; set; } = new Filters();
        private TimeSpanAxis timeSpanAxis;
        public TimeSpanAxis TimeSpanAxis { get => timeSpanAxis; set => SetAndNotify(ref timeSpanAxis, value); }
        private BindableCollection<IRenderableSeriesViewModel> renderableSeriesViewModels = new BindableCollection<IRenderableSeriesViewModel>();
        public BindableCollection<IRenderableSeriesViewModel> RenderableSeries { get => renderableSeriesViewModels; set => SetAndNotify(ref renderableSeriesViewModels, value); }
        public List<LightMode<TimeSpan, double>> LightModes { get; set; } = new List<LightMode<TimeSpan, double>>();
        private ScrollingViewportManager viewportManager = new ScrollingViewportManager(10);
        public ScrollingViewportManager ViewportManager { get => viewportManager; set => SetAndNotify(ref viewportManager, value); }
        private IRange xrange = new TimeSpanRange(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10));
        public IRange XVisibleRange { get => xrange; set => SetAndNotify(ref xrange, value); }
        private IRange yrange = new DoubleRange(0, 10);
        public IRange YVisibleRange { get => yrange; set => SetAndNotify(ref yrange, value); }
    }
    public class DioChannel : ChannelBase
    {
        public bool Offset { get; set; }
        public Filters Filters { get; set; } = new Filters();
    }
    public class LightMode<TX, TY> : PropertyChangedBase
        where TX : IComparable
        where TY : IComparable
    {
        /// <summary>
        /// 0 1 2 3
        /// </summary>
        public int LightType { get; set; }
        public double OffsetValue { get; set; }
        private XyDataSeries<TX, TY> dataSeries = new XyDataSeries<TX, TY>();
        public XyDataSeries<TX, TY> XyDataSeries { get => dataSeries; set => SetAndNotify(ref dataSeries, value); }
        public Derivative Derivative { get; set; } = new Derivative(30);
        public SolidColorBrush WaveColor { get; set; }
    }
    public class Filters
    {
        public bool IsLowPass { get; set; }
        public double LowPass { get; set; }
        public bool IsHighPass { get; set; }
        public double HighPass { get; set; }
        public bool IsNotch { get; set; }
        public double Notch { get; set; }
        public bool IsBandpass { get; set; }
        public double Bandpass1 { get; set; } = 0;
        public double Bandpass2 { get; set; } = 0;
        public bool IsBandstop { get; set; }
        public double Bandstop1 { get; set; } = 0;
        public double Bandstop2 { get; set; } = 0;
        public bool IsSmooth { get; set; } = true;
        public double Smooth { get; set; } = 5;
        public OnLineFilterTool OnlineFilter { get; set; } = new OnLineFilterTool();
    }
}
