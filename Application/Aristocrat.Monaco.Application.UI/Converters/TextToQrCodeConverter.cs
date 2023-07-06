namespace Aristocrat.Monaco.Application.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using Helpers;

    /// <summary>
    ///     Convert text to a QR code
    /// </summary>
    public class TextToQrCodeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
            {
                throw new ArgumentException();
            }

            var text = (string)values[0];
            var darkBrush = (SolidColorBrush)values[1];
            var lightBrush = (SolidColorBrush)values[2];
            var code = InspectionSummaryQrCodeProvider.GetXamlImage(text, darkBrush.Color, lightBrush.Color);
            return code;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
