namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Converts a list of values to a Visibility enumeration based on the equality of the values.
    /// </summary>
    internal class EqualityVisibilityConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Gets or sets the return value when all values are equal.
        /// </summary>
        public Visibility Equal { get; set; } = Visibility.Visible;

        /// <summary>
        ///     Gets or sets the return value when all values are not equal.
        /// </summary>
        public Visibility NotEqual { get; set; } = Visibility.Collapsed;

        /// <inheritdoc />
        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.Length == 0)
            {
                return NotEqual;
            }

            if (value.Length == 1)
            {
                return Equal;
            }

            return value.All(x => x.Equals(value[0])) ? Equal : NotEqual;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
