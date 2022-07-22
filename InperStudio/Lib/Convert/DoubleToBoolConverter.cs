using System;
using System.Globalization;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class DoubleToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool res = false;
            if (value.ToString() == "NaN")
            {
                res = true;
            }
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return double.NaN;
        }
    }
}
