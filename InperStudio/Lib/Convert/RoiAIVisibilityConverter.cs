﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class RoiAIVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = Visibility.Collapsed;

            if (((string)value).Contains("ROI") || ((string)value).StartsWith("AI"))
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
