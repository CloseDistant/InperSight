﻿using InperStudio.Lib.Enum;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace InperStudio.Lib.Convert
{
    public class ChannelTypeToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isEnable = false;
            if (value.ToString().Equals(ChannelTypeEnum.Camera.ToString()))
            {
                isEnable = true;
            }
            return isEnable;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}