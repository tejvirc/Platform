namespace Aristocrat.Monaco.Application.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    /// <summary>
    ///     IntToStringWithDefaultConverter class.  Designed to be used with TextBox that is bound to an integer property.
    ///     Allows one to specify a Default Value to use in the case where the Text field is cleared or empty.  This allows the
    ///     binding to still work in the case where the TextBox is cleared by the user (by default the binding would fail since
    ///     it
    ///     could not covert an empty string to an integer).  In this case your bounded property will be assigned the
    ///     EmptyStringValue
    ///     (specified Default value).  All other cases will work much like the WPF default converter.
    /// </summary>
    public class IntToStringWithDefaultConverter : IValueConverter
    {
        /// <summary>
        ///     Gets or sets EmptyStringValue
        /// </summary>
        public int EmptyStringValue { get; set; }

        /// <summary>
        ///     <method>Convert</method>.
        /// </summary>
        /// <param name="value">
        ///     value of field.
        /// </param>
        /// <param name="targetType">
        ///     targetType.
        /// </param>
        /// <param name="parameter">
        ///     parameter
        /// </param>
        /// <param name="culture">
        ///     culture
        /// </param>
        /// <returns>
        ///     returns converted object
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return null;
                case string _:
                    return value;
                case int underlyingValue when underlyingValue == EmptyStringValue:
                    return string.Empty;
                default:
                    return value.ToString();
            }
        }

        /// <summary>
        ///     <method>ConvertBack</method>.
        /// </summary>
        /// <param name="value">
        ///     value of field.
        /// </param>
        /// <param name="targetType">
        ///     targetType.
        /// </param>
        /// <param name="parameter">
        ///     parameter
        /// </param>
        /// <param name="culture">
        ///     culture
        /// </param>
        /// <returns>
        ///     returns converted object
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string underlyingValue)
            {
                return int.TryParse(underlyingValue, out _) ? System.Convert.ToInt32(underlyingValue) : EmptyStringValue;
            }

            return EmptyStringValue;
        }
    }
}