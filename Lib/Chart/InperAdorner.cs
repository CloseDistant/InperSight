using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace InperSight.Lib.Chart
{
    public class InperAdorner : Adorner
    {
        private string prompt;
        private Brush brush;
        private double left;
        private double top;
        public InperAdorner(UIElement adornedElement, string _prompt, Brush _brush, double _left, double _top) : base(adornedElement)
        {
            prompt = _prompt;
            brush = _brush;
            left = _left;
            top = _top;
        }
        [Obsolete]
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawText(new FormattedText(prompt, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, brush), new Point(left, top));
        }
    }
}
