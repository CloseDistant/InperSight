using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class CombinedVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool isComboBoxOpen = (bool)values[0];
            int isCanvasEmpty = (int)(values[1] as UIElementCollection).Count;
            bool isMouseOver = (bool)values[2];
            // 根据两个条件的结果确定最终的可见性状态
            return (isComboBoxOpen && isCanvasEmpty > 0) || isMouseOver ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
