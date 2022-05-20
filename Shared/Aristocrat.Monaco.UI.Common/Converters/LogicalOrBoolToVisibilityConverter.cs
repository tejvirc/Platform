﻿namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Converts booleans into Visibility settings, using logical 'or'
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class LogicalOrBoolToVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Gets or sets the true value
        /// </summary>
        public Visibility TrueValue { get; set; } = Visibility.Visible;

        /// <summary>
        ///     Gets or sets the false value
        /// </summary>
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return values.Any(value => value is bool b && b) ? TrueValue : FalseValue;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(LogicalOrBoolToVisibilityConverter)} is only one-way converter");
        }
    }
}
