namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     Converts a boolean value to its localized form
    /// </summary>
    public class BoolToStringConverter : IValueConverter
    {
        private ILocalizer OperatorLocalizer => Localizer.For(CultureFor.Operator);

        private string TrueString => OperatorLocalizer.GetString(ResourceKeys.TrueText);

        private string FalseString => OperatorLocalizer.GetString(ResourceKeys.FalseText);

        /// <summary>
        ///     Convert a bool to a localized string
        /// </summary>
        /// <param name="value">bool value to convert</param>
        /// <param name="targetType">string</param>
        /// <param name="parameter">not used</param>
        /// <param name="culture">not used</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return boolean ? TrueString : FalseString;
            }

            return OperatorLocalizer.GetString(ResourceKeys.NotAvailable);
        }

        /// <summary>
        ///     Not used
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
