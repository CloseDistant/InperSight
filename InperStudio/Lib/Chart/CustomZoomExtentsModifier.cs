using SciChart.Charting;
using SciChart.Charting.ChartModifiers;
using SciChart.Core.Utility.Mouse;
using System;
using System.Windows;

namespace InperStudio.Lib.Chart
{
    public class CustomZoomExtentsModifier : ChartModifierBase
    {
        private Point? _lastPoint;

        public override void OnModifierMouseDown(ModifierMouseArgs e)
        {
            base.OnModifierMouseDown(e);

            e.Handled = true;
            _lastPoint = e.MousePoint;
        }
        public override void OnModifierDoubleClick(ModifierMouseArgs e)
        {
            base.OnModifierDoubleClick(e);
            ParentSurface.AnimateZoomExtentsX(TimeSpan.FromMilliseconds(200));
            if (_lastPoint == null) return;

            var currentPoint = e.MousePoint;
            var xDelta = currentPoint.X - _lastPoint.Value.X;
            var yDelta = _lastPoint.Value.Y - currentPoint.Y;

            using (ParentSurface.SuspendUpdates())
            {
                // Scroll the XAxis by the number of pixels since the last update
                _ = XAxis.Scroll(XAxis.IsHorizontalAxis ? xDelta : -yDelta, ClipMode.None);

                // Scroll the YAxis by the number of pixels since the last update
                _ = YAxis.Scroll(YAxis.IsHorizontalAxis ? -xDelta : yDelta, ClipMode.None);

                // Note.. can be extended for multiple YAxis XAxis, just iterate over all axes on the parent surface
            }

            _lastPoint = currentPoint;
        }
        public override void OnModifierMouseMove(ModifierMouseArgs e)
        {
            base.OnModifierMouseMove(e);

        }

        public override void OnModifierMouseUp(ModifierMouseArgs e)
        {
            base.OnModifierMouseUp(e);
            _lastPoint = null;
        }
    }
}
