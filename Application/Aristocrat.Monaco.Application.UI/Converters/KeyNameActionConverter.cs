namespace Aristocrat.Monaco.Application.UI.Converters
{
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
                case "Operator Switch":
                    r = "Operator";
                    break;
                case "Jackpot SW":
                    r = "Technician";
                    break;
                case "Up":
                    r = "Off";
                    break;
                case "Down":
                    r = "On";
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