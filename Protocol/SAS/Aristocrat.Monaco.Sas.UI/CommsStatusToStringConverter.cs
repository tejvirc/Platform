namespace Aristocrat.Monaco.Sas.UI
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Application.Contracts.Localization;
    using Aristocrat.Sas.Client;
    using Localization.Properties;

    /// <summary>
    ///     A converter class to translation CommsStatus using localization
    /// </summary>
    public class CommsStatusToStringConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CommsStatus commsStatus))
            {
                return string.Empty;
            }

            var result = string.Empty;

            switch (commsStatus)
            {
                case CommsStatus.Online:
                    result = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasOnline);
                    break;
                case CommsStatus.Offline:
                    result = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasOffline);
                    break;
                case CommsStatus.LoopBreak:
                    result = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.SasLoopBreak);
                    break;
            }

            return result;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}