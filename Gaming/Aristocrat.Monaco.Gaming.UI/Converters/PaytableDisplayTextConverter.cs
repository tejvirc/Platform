namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;
    using Contracts;
    using Contracts.Progressives;

    /// <summary>
    ///     Takes multiple parameters to determine how the paytable rtp string should be displayed as
    /// </summary>
    public class PaytableDisplayTextConverter : IMultiValueConverter
    {
        /// <summary>
        ///     Convert from gameDetails and bool to string displaying paytable rtp
        /// </summary>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var rtpRange = values[0] as RtpRange; // GameDetail RtpRange
            if (rtpRange == null) return string.Empty;

            var variationId = (string)values[1]; // GameDetail VariationId
            var gameRtpAsRange = (bool)values[2]; // ShowGameRtpAsRange

            StringBuilder paytableDisplayText = new StringBuilder();

            paytableDisplayText.Append(gameRtpAsRange ? rtpRange.ToString() : rtpRange.Minimum.GetRtpString());
            if (!string.IsNullOrEmpty(variationId)) paytableDisplayText.Append($" (v{variationId})");

            return paytableDisplayText.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}