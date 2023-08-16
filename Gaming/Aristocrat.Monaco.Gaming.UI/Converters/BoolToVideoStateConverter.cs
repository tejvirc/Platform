namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using ManagedBink;
    using Models;

    public class BoolToVideoStateConverter : IValueConverter
    {
        /// <summary>
        ///     Gets or sets the true value
        /// </summary>
        public BinkVideoState TrueValue { get; set; } = BinkVideoState.Playing;

        /// <summary>
        ///     Gets or sets the false value
        /// </summary>
        public BinkVideoState FalseValue { get; set; } = BinkVideoState.Stopped;

        /// <summary>
        ///     Converts bool value to video state.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = value is bool boolValue && boolValue ? TrueValue : FalseValue;
            return result;
        }
        /// <summary>
        ///     Converts video state to bool value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(TrueValue);
        }
    }
}