﻿namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Contracts;

    /// <summary>
    ///     ReturnToPlayerConverter
    /// </summary>
    public class ReturnToPlayerConverter : IValueConverter
    {
        /// <summary>
        ///     Convert
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case Tuple<decimal, decimal> value1:
                    return value1.Item1 == value1.Item2
                        ? value1.Item1.GetRtpString()
                        : GameConfigHelper.GetRtpRangeString(value1.Item1, value1.Item2);
                case decimal value2:
                    return value2.GetRtpString();
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        ///     ConvertBack
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}