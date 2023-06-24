namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    ///     OperatorFormattedCurrencyConverter
    /// </summary>
    /// <remarks>
    ///     This is for converting millicents to a formatted currency value
    ///     If you pass in a converter Parameter of ZeroNotAvailable, then 0 will be mapped to N/A
    /// </remarks>
    public class OperatorFormattedCurrencyConverter : IValueConverter
    {
        private const string ZeroNotAvailable = "ZeroNotAvailable";

        private const string ZeroNoLimit = "ZeroNoLimit";

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is long currency)
            {
                if (currency == 0)
                {
                    return (parameter as string) switch
                    {
                        ZeroNotAvailable => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable),
                        ZeroNoLimit => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit),
                        _ => currency.MillicentsToDollars().FormattedCurrencyStringForOperator(),
                    };
                }

                return currency.MillicentsToDollars().FormattedCurrencyStringForOperator();
            }

            if (value is decimal currency1)
            {
                return currency1.FormattedCurrencyStringForOperator();
            }

            if (value is double currency2)
            {
                return currency2.FormattedCurrencyStringForOperator();
            }

            return string.Empty;
        }

        /// <inheritdoc />
        /// <exception cref="NotImplementedException">Thrown when the requested operation is unimplemented.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}