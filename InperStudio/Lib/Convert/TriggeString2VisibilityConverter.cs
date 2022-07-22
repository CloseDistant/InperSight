﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class TriggeString2VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Collapsed;

            if (value != null && ((string)value).Contains("Edge"))
            {
                visibility = Visibility.Visible;
            }
            return visibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
