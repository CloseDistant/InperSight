using System;
using System.Globalization;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class VideoIntToTIme : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long sec = (long)value;

            return formatLongToTimeStr(sec);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public string formatLongToTimeStr(long time)
        {
            int hour = (int)(time / 3600);
            int min = (int)((time - hour * 3600) / 60);
            int sen = (int)(time - hour * 3600 - min * 60);

            string _hour = hour < 10 ? "0" + hour : hour.ToString();
            string _min = min < 10 ? "0" + min : min.ToString();
            string _sen = sen < 10 ? "0" + sen : sen.ToString();

            return _hour + ":" + _min + ":" + _sen;
        }
    }
}
