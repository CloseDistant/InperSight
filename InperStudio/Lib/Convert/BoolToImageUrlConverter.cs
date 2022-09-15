using SciChart.Charting.Visuals.Axes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class BoolToImageUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string res = string.Empty;
            if ((bool)value)
            {
                res = "/Lib/Images/New/split.png";

            }
            else
            {
                res = "/Lib/Images/New/merge.png";
            }
            return res;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
