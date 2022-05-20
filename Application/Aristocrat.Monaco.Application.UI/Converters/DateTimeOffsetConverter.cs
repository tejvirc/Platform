namespace Aristocrat.Monaco.Application.UI.Converters
{
    using Contracts;
    using Kernel;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class DateTimeOffsetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is DateTime dateTime &&
                values[1] is TimeSpan offset)
            {
                var services = ServiceManager.GetInstance();
                var timeService = services.GetService<ITime>();

                return timeService.FormatDateTimeString(dateTime.Add(offset));
            }

            return string.Empty;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
