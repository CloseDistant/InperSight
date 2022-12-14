using InperSight.ViewModels;
using SciChart.Charting.Visuals.Axes;
using SciChart.Charting.Visuals.Axes.LabelProviders;
using System;

namespace InperSight.Lib.Chart
{
    public class CustomTimeSpanLableProvider : TimeSpanLabelProvider
    {
        public override void Init(IAxisCore parentAxis)
        {
            base.Init(parentAxis);
        }
        public override void OnBeginAxisDraw()
        {
            base.OnBeginAxisDraw();
        }
        public override ITickLabelViewModel CreateDataContext(IComparable dataValue)
        {
            return base.CreateDataContext(dataValue);
        }
        public override string FormatLabel(IComparable dataValue)
        {
            var value = (TimeSpan)dataValue;
            if (DataShowControlViewModel.TextFormat.Equals("hh:mm:ss"))
            {
                return dataValue.ToString();
            }
            if (DataShowControlViewModel.TextFormat.Equals("ms"))
            {
                return value.TotalMilliseconds.ToString();
            }
            if (DataShowControlViewModel.TextFormat.Equals("Seconds"))
            {
                return value.TotalSeconds.ToString();
            }
            if (DataShowControlViewModel.TextFormat.Equals("Time of day"))
            {
                return value.TotalDays.ToString();
            }
            return dataValue.ToString();
        }
        public override string FormatCursorLabel(IComparable dataValue)
        {
            return dataValue.ToString();
        }
    }
}
