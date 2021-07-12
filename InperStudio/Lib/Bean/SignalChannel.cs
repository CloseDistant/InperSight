using OpenCvSharp;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Data.Model;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace InperStudio.Lib.Bean
{
    public abstract class SignalChannel
    {
        public int ChannelId { get; set; }
        public string ChannelType { get; set; }
        public string Name { get; set; }
        public bool Offset { get; set; }
        public SolidColorBrush BgColor { get; set; } = Brushes.Red;
        public IRange YaxisDoubleRange { get; set; }
        public string Hotkeys { get; set; }
        public Filters Filters { get; set; }
    }
    public class Filters
    {
        public double LowPass { get; set; }
        public double HighPass { get; set; }
        public double Notch { get; set; }
        public double Smooth { get; set; }
    }

    public class SignalCameraChannel : SignalChannel
    {
        public int Roi { get; set; }

    }
    public class SignalAnalogChannel : SignalChannel
    {

    }
}
