using System;
using System.Globalization;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    class DoubleToBoolReConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool res = true;
            if (value.ToString() == "NaN")
            {
                res = false;
            }
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return 0;
        }
    }
}
