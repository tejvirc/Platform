namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     This converter takes a list and by default returns Collapsed if the list is empty, Visible if the list has any
    ///     items
    /// </summary>
    public class ListToVisibilityConverter : IValueConverter
    {
        /// <summary>
        ///     Gets or sets the true value
        /// </summary>
        public Visibility TrueValue { get; set; } = Visibility.Visible;

        /// <summary>
        ///     Gets or sets the false value
        /// </summary>
        public Visibility FalseValue { get; set; } = Visibility.Collapsed;

        /// <summary>
        ///     Convert from list to a visibility state
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is IList list && list.Count > 0 ? TrueValue : FalseValue;
        }

        /// <summary>
        ///     Not implemented
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}