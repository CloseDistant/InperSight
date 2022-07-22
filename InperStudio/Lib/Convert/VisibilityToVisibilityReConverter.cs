using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class VisibilityToVisibilityReConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility;
            if ((Visibility)value == Visibility.Visible)
            {
                visibility = Visibility.Collapsed;
            }
            else
            {
                visibility = Visibility.Visible;
            }
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
