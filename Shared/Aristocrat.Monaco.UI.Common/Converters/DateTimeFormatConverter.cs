namespace Aristocrat.Monaco.UI.Common.Converters
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Application.Contracts;
    using Kernel;

    /// <summary>
    ///     DateTimeFormatConverter
    /// </summary>
    public class DateTimeFormatConverter : IValueConverter
    {
        private const string MinDateBlankParameter = "MinDateBlank";

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue)
            {
                return value;
            }

            if (!TryGetDateTime(value, out var dateTime))
            {
                return string.Empty;
            }

            if (dateTime == DateTime.MinValue && parameter is string param && param == MinDateBlankParameter)
            {
                return string.Empty;
            }
            var serviceManager = ServiceManager.GetInstance();
            var dateFormat = serviceManager.GetService<IPropertiesManager>().GetValue(
                ApplicationConstants.LocalizationOperatorDateFormat,
                ApplicationConstants.DefaultDateFormat);
            return serviceManager.GetService<ITime>().GetFormattedLocationTime(dateTime, $"{dateFormat} {ApplicationConstants.DefaultTimeFormat}");
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (TryGetDateTime(value, out var dateTime))
            {
                return dateTime;
            }

            return null;
        }

        private static bool TryGetDateTime(object data, out DateTime dateTime)
        {
            switch (data)
            {
                case string stringData:
                    return DateTime.TryParse(stringData, out dateTime);
                case DateTime dateTimeData:
                    dateTime = dateTimeData;
                    return true;
                default:
                    dateTime = DateTime.MinValue;

                    return false;
            }
        }
    }
}