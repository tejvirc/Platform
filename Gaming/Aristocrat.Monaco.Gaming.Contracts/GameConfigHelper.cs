namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Localization.Properties;
    using Progressives;

    /// <summary>
    /// Utility class for creating RTP strings
    /// </summary>
    public static class GameConfigHelper
    {
        /// <summary>
        /// Creates the RTP string from RTP min and max values.
        /// </summary>
        /// <param name="rtpRange">The rtp range</param>
        /// <returns></returns>
        public static string GetRtpRangeString(RtpRange rtpRange)
        {
            return GetRtpRangeString(rtpRange?.Minimum, rtpRange?.Maximum);
        }

        /// <summary>
        /// Creates the RTP string from RTP min and max values.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static string GetRtpRangeString(decimal? min, decimal? max)
        {
            if (min == null || max == null)
            {
                return Resources.NoLimit;
            }

            string rtpString;

            if(max == int.MaxValue && min == int.MinValue)
            {
                rtpString = Resources.NoLimit;
            }
            else if (max == int.MaxValue)
            {
                rtpString = $"{Resources.AtLeast} {min.Value.GetRtpString()}";
            }
            else if (min == int.MinValue)
            {
                rtpString = $"{Resources.AtMost} {max.Value.GetRtpString()}";
            }
            else
            {
                rtpString = $"{min.Value.GetRtpString()} - {max.Value.GetRtpString()}";
            }

            return rtpString;
        }

        /// <summary>
        /// Converts the Rtp value to the required format string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetRtpString(this decimal value)
        {
            return (value / 100).ToString("P2");
        }
    }
}