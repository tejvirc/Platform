namespace Aristocrat.Monaco.Gaming.Lobby.Converters
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
        public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
            {
                return NotEqual;
            }

            if (values.Length == 1)
            {
                return Equal;
            }

            return values.All(x => x.Equals(values[0])) ? Equal : NotEqual;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
