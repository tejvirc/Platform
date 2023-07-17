namespace Aristocrat.Monaco.Application.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using Contracts.Localization;
    using Monaco.Localization.Properties;

    /// <summary>
    ///     Device Status string is used to determine the color of the text
    /// Gray = Currently validating or not yet validated
    /// Yellow = Warning (no error found on discovery but a problem has occurred, such as timeout, invalid port, etc.)
    /// Red = Error
    /// Green = Successful discovery
    /// Blue = All other statuses (default text color)
    /// </summary>
    public class DeviceStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush(Color.FromArgb(0xff, 0x68, 0xd2, 0xf1));
            }

            var status = value.ToString();
            if (status.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Validating)) || status.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotValidated)) ||
                status.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Searching)))
            {
                return Brushes.Gray;
            }

            if (status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WarningText)) || status.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidProtocol)) ||
                status.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidPort)))
            {
                return Brushes.Yellow;
            }

            if (status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ErrorText)) || status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Failed)) ||
                status.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoDeviceDetected)) || status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.InvalidDeviceDetectedTemplate)))
            {
                return Brushes.Red;
            }

            if (status.Contains(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ConnectedText)) || status.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.HardwareDiscoveryCompleteLabel)) ||
                status.Equals(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.DeviceDetected)))
            {
                return Brushes.LimeGreen;
            }

            return new SolidColorBrush(Color.FromArgb(0xff, 0x68, 0xd2, 0xf1));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
