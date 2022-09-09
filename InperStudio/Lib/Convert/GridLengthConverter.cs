using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;

namespace InperStudio.Lib.Convert
{
    public class GridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)(value ?? false))
            {
                return new GridLength(1, GridUnitType.Star);
            }
            else
            {
                return new GridLength(0);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
