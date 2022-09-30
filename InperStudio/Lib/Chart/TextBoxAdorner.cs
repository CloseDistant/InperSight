using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace InperStudio.Lib.Chart
{
    public class TextBoxAdorner : Adorner
    {
        private string prompt;
        public TextBoxAdorner(UIElement adornedElement,string _prompt) : base(adornedElement)
        {
            prompt = _prompt;
        }
        [Obsolete]
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            drawingContext.DrawText(new FormattedText(prompt, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Red), new Point(0, -15));
        }
    }
}
