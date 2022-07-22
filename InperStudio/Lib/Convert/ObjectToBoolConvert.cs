using System;
using System.Globalization;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class ObjectToBoolConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool enable = false;
            try
            {
                enable = value != null;
            }
            catch (Exception ex)
            {
                App.Log.Error(ex.ToString());
            }
            return enable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
