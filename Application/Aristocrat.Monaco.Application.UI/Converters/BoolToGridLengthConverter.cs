namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    ///     Converts bool to string property.  By setting the TrueValue
    ///     and False value properties, the default behavior can be customized.
    ///     In particular, you can use this to invert the default behavior to
    ///     make something collapsed when true.
    ///     Xaml usage:
    ///     converters:BoolToGridLengthConverter x:Key="BoolToGridLengthValue" TrueValue="0.5*" FalseValue="0.0*"
    /// </summary>
    public class BoolToGridLengthConverter : IValueConverter
    {
        /// <summary>
        ///     Gets or sets the true value
        /// </summary>
        public GridLength TrueValue { get; set; }

        /// <summary>
        ///     Gets or sets the false value
        /// </summary>
        public GridLength FalseValue { get; set; }

        /// <summary>
        ///     Convert from bool to a GridLength
        /// </summary>
        /// <param name="value">the bool value to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>a GridLength</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool == false)
            {
                return FalseValue;
            }

            return (bool)value ? TrueValue : FalseValue;
        }

        /// <summary>
        ///     Convert a GridLength back to bool
        /// </summary>
        /// <param name="value">the visibility state to convert</param>
        /// <param name="targetType">not used</param>
        /// <param name="parameter">also not used</param>
        /// <param name="culture">more not used</param>
        /// <returns>true, false, or null if value wasn't a GridLength</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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