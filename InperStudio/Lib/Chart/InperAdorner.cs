using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace InperStudio.Lib.Chart
{
    public class InperAdorner : Adorner
    {
        private string prompt;
        private Brush brush;
        private double left;
        private double top;
        private readonly bool isWeight;

        public InperAdorner(UIElement adornedElement, string _prompt, Brush _brush, double _left, double _top, bool _isWeight = false) : base(adornedElement)
        {
            prompt = _prompt;
            brush = _brush;
            left = _left;
            top = _top;
            isWeight = _isWeight;
        }
        [Obsolete]
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var format = new FormattedText(prompt, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, brush);
            if (isWeight)
            {
                format.SetFontWeight(FontWeights.Bold);
                format.SetFontSize(16);
                left -= 4;
            }
            drawingContext.DrawText(format, new Point(left, top));
        }
    }
}
