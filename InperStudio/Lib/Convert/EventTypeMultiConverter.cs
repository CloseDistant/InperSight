using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
namespace InperStudio.Lib.Convert
{
    public class EventTypeMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((((string)values[0]).Contains("AI") || ((string)values[0]).Contains("ROI")) && ((string)values[1]).Contains("Marker"))
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
