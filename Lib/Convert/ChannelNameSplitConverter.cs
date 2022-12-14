using System;
using System.Globalization;
using System.Windows.Data;

namespace InperSight.Lib.Convert
{
    public class ChannelNameSplitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return value;
            string text = value.ToString();
            if (text.EndsWith("-"))
            {
                text = text.Substring(0, text.Length - 1);
            }

            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
