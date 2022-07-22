using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace InperStudioControlLib.Attach
{
    public class VisibilityElement
    {
        public static readonly DependencyProperty VisibilityProperty = DependencyProperty.RegisterAttached(
            "Visibility", typeof(Visibility), typeof(VisibilityElement), new FrameworkPropertyMetadata(Visibility.Visible, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetVisibility(DependencyObject element, Visibility value) => element.SetValue(VisibilityProperty, value);

        public static Visibility GetVisibility(DependencyObject element) => (Visibility)element.GetValue(VisibilityProperty);
    }
}
