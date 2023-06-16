namespace Aristocrat.Monaco.Application.UI.Converters
{
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Aristocrat.Monaco.Localization.Properties;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class KeyNameActionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value?.ToString();
            var r = str;
            switch (str)
            {
                case "Operator":
                case "Operator Switch":
                    r = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.OperatorRoleName);
                    break;
                case "Jackpot SW":
                    r = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TechnicianRoleName);
                    break;
                case "Off":
                case "Up":
                    r = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Off);
                    break;
                case "On":
                case "Down":
                    r = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.On);
                    break;
                default:
                    break;
            }

            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}