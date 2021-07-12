using HandyControl.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace InperStudioControlLib.Control.Border
{
    public class InperTagBorder : System.Windows.Controls.Border
    {
        static InperTagBorder()
        {
            StyleProperty.OverrideMetadata(typeof(InperTagBorder), new FrameworkPropertyMetadata(ResourceHelper.GetResource<Style>("InperTagStyle")));
        }


        #region DependencyProperty
        public static readonly DependencyProperty InperTagTitleTextProperty = DependencyProperty.Register(
           "InperTagTitleText", typeof(string), typeof(InperTagBorder), new PropertyMetadata("Inper"));
        public string InperTagTitleText
        {
            get => (string)GetValue(dp: InperTagTitleTextProperty);
            set => SetValue(dp: InperTagTitleTextProperty, value);
        }
        public static readonly DependencyProperty InperTagTitleBackgroundProperty = DependencyProperty.Register(
           "InperTagTitleBackground", typeof(SolidColorBrush), typeof(InperTagBorder), new PropertyMetadata("#8400FF"));
        public SolidColorBrush InperTagTitleBackground
        {
            get => (SolidColorBrush)GetValue(dp: InperTagTitleBackgroundProperty);
            set => SetValue(dp: InperTagTitleBackgroundProperty, value);
        }
        public static readonly DependencyProperty InperTagMainBackgroundProperty = DependencyProperty.Register(
          "InperTagMainBackground", typeof(SolidColorBrush), typeof(InperTagBorder), new PropertyMetadata(Brushes.White));
        public SolidColorBrush InperTagMainBackground
        {
            get => (SolidColorBrush)GetValue(dp: InperTagMainBackgroundProperty);
            set => SetValue(dp: InperTagMainBackgroundProperty, value);
        }
        public static readonly DependencyProperty InperTagTextForegroundProperty = DependencyProperty.Register(
           "InperTagTextForeground", typeof(Brush), typeof(InperTagBorder), new PropertyMetadata(Brushes.White));
        public Brush InperTagTextForeground
        {
            get => (Brush)GetValue(dp: InperTagTextForegroundProperty);
            set => SetValue(dp: InperTagTextForegroundProperty, value);
        }
        public static readonly DependencyProperty InperTagCornerRadiusProperty = DependencyProperty.Register(
          "InperTagCornerRadius", typeof(CornerRadius), typeof(InperTagBorder), new PropertyMetadata(10));
        public CornerRadius InperTagCornerRadius
        {
            get => (CornerRadius)GetValue(dp: InperTagCornerRadiusProperty);
            set => SetValue(dp: InperTagCornerRadiusProperty, value);
        }
        #endregion

    }
}
