using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class HalfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && parameter is string strMaxWidth && double.TryParse(strMaxWidth, out double maxWidth))
            {
                double halfWidth = width / 2;
                return halfWidth > maxWidth ? maxWidth : halfWidth;
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
