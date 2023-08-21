namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     This class defines a value converter that takes multiple boolean values and performs logical AND on the values
    ///     <remarks>If a value type is not boolean, it is treated as a FALSE boolean value</remarks>
    /// </summary>
    [ValueConversion(typeof(bool), typeof(bool))]
    public class LogicalAndConverter : IMultiValueConverter
    {
        /// <inheritdoc />
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            foreach (var value in values)
            {
                if (!(value is bool) || !(bool)value)
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(LogicalAndConverter)} is only one-way converter");
        }
    }
}