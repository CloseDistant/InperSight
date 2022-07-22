using InperStudio.Lib.Enum;
using System;
using System.Globalization;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class ManualToMarkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.ToString() == ChannelTypeEnum.Manual.ToString())
            {
                return "Marker";
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
