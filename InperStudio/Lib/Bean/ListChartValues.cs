using SciChart.Charting.ViewportManagers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stylet;
using System.Threading.Tasks;
using SciChart.Charting.ChartModifiers;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.Axes;
using SciChart.Data.Model;
using SciChart.Charting.Visuals.TradeChart;
using System.Windows.Input;

namespace InperStudio.Lib.Bean
{
    public class ListChartValues<TX, TY> : PropertyChangedBase
         where TX : IComparable
         where TY : IComparable
    {
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string groupId;
        public string GroupId { get => groupId; set => SetAndNotify(ref groupId, value); }

        private IViewportManager viewportManager;
        public IViewportManager ViewportManager { get { return viewportManager; } set { SetAndNotify(ref viewportManager, value); } }

        private XyDataSeries<TX, TY> s0dataSeries = new XyDataSeries<TX, TY>();
        public XyDataSeries<TX, TY> S0DataSeries { get => s0dataSeries; set => SetAndNotify(ref s0dataSeries, value); }

        private TimeSpanRange _XVisibleRange;
        public TimeSpanRange XVisibleRange { get => _XVisibleRange; set => SetAndNotify(ref _XVisibleRange, value); }
    }
}
