namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Convert a string to a wrapped-text string by replacing spaces with newlines
    /// </summary>
    public class WrappedTextConverter : IValueConverter
    {
        /// <summary>
        ///     Replace spaces with newlines
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : value.ToString().Replace(' ', '\n');
        }

        /// <summary>
        ///     Not used
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
