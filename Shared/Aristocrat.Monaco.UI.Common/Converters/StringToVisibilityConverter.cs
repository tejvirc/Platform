namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using Quartz.Util;
    using IValueConverter = System.Windows.Data.IValueConverter;

    /// <summary>
    ///     Converts a string value into a Visibility value (if the string is null/empty/whitespace returns Collapsed)
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///     Gets or sets the null/empty/whitespace
        /// </summary>
        public Visibility EmptyValue { get; set; } = Visibility.Collapsed;

        /// <summary>
        ///     Gets or sets the not null/empty/whitespace value
        /// </summary>
        public Visibility NotEmptyValue { get; set; } = Visibility.Visible;

        /// <summary>
        ///     Convert from string to Visibility
        /// </summary>
        /// <returns>Collapsed if string is null/empty/whitespace; Visible otherwise</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null || value.ToString().IsNullOrWhiteSpace() ? EmptyValue : NotEmptyValue;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
