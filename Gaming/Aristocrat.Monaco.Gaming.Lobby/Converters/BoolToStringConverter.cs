namespace Aristocrat.Monaco.Gaming.Lobby.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     Converts bool to string property.  By setting the TrueValue
    ///     and False value properties, the string mappings can be customized.
    /// </summary>
    internal class BoolToStringConverter : IValueConverter
    {
        /// <summary>
        ///     Gets or sets the true value string
        /// </summary>
        public string TrueValue { get; set; } = "True";

        /// <summary>
        ///     Gets or sets the false value string
        /// </summary>
        public string FalseValue { get; set; } = "False";

        /// <summary>
        ///     Convert from bool to a string
        /// </summary>
        /// <param name="value">the bool value to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a true or false string</returns>
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool == false)
            {
                return null;
            }

            return (bool)value ? TrueValue : FalseValue;
        }

        /// <summary>
        ///     Convert a true false string back to bool
        /// </summary>
        /// <param name="value">the true/false string to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>true, false, or null if value wasn't true/false</returns>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (Equals(value, TrueValue))
            {
                return true;
            }

            if (Equals(value, FalseValue))
            {
                return false;
            }

            return null;
        }
    }
}
