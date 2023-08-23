namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Application.Contracts.Localization;
    using Application.Contracts.Extensions;
    using Localization.Properties;
    using Progressives;

    /// <summary>
    ///     Utility class for creating RTP strings
    /// </summary>
    public static class GameConfigHelper
    {
        private const decimal Precision = 1000M;

        /// <summary>
        ///     Creates the RTP string from RTP min and max values.
        /// </summary>
        /// <param name="rtpRange">The rtp range</param>
        /// <returns></returns>
        public static string GetRtpRangeString(RtpRange rtpRange)
        {
            return GetRtpRangeString(rtpRange?.Minimum, rtpRange?.Maximum);
        }

        /// <summary>
        ///     Creates the RTP string from RTP min and max values.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static string GetRtpRangeString(decimal? min, decimal? max)
        {
            if (min == null || max == null)
            {
                return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);
            }

            string rtpString;

            min = min == int.MinValue ? min : ConvertToRtp((decimal)min);
            max = max == int.MaxValue ? max : ConvertToRtp((decimal)max);
            if (max == int.MaxValue && min == int.MinValue)
            {
                rtpString = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoLimit);
            }
            else if (max == int.MaxValue)
            {
                rtpString = $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AtLeast)} {min.Value.GetRtpString()}";
            }
            else if (min == int.MinValue)
            {
                rtpString = $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.AtMost)} {max.Value.GetRtpString()}";
            }
            else
            {
                rtpString = $"{min.Value.GetRtpString()} - {max.Value.GetRtpString()}";
            }

            return rtpString;
        }

        /// <summary>
        ///     Converts an Rtp value to percentage if not already in the form.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal ConvertToRtp(decimal value)
        {
            if (value <= 100)
            {
                return value; // Already in percent value
            }

            // On platform a precision of 1000 is maintained. For ex: 75.127% is represented as 75127.
            return value / Precision;
        }

        /// <summary>
        ///     Converts the Rtp value to the required format string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetRtpString(this decimal value)
        {
            return (value / 100).ToPercentageFormattedString();
        }

        /// <summary>
        ///     Converts the RTP value and returns the formatted string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetConvertedRtpString(decimal value)
        {
            return ConvertToRtp(value).GetRtpString();
        }
    }
}