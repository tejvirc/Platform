namespace Aristocrat.Monaco.Gaming.UI.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using Contracts;
    using Contracts.Rtp;

    /// <summary>
    ///     RtpToHoldConverter
    /// </summary>
    public class RtpToHoldConverter : IValueConverter
    {
        private const decimal OneHundredPercent = 100m;

        /// <summary>
        ///     Convert
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return string.Empty;
                case Tuple<decimal, decimal> value1:
                    return value1.Item1 == value1.Item2
                        ? (OneHundredPercent - value1.Item1).ToRtpString()
                        : new RtpRange(OneHundredPercent - value1.Item2, OneHundredPercent - value1.Item1).ToString();
                case decimal value2:
                    return (OneHundredPercent - value2).ToRtpString();
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        ///     ConvertBack
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}