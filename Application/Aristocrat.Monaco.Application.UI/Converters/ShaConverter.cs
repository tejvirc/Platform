﻿namespace Aristocrat.Monaco.Application.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Windows.Data;

    /// <summary>
    /// Convert between 40-character text and formatted text (dividing it into blocks of four characters)
    /// </summary>
    public class ShaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "" : Regex.Replace(value.ToString(), ".{4}", "$0 ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "" : value.ToString().Replace(" ", "");
        }
    }
}
