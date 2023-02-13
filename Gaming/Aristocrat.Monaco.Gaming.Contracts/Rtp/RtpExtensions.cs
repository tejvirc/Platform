namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System;

    /// <summary>
    ///     A set of utility methods to deal with Rtp (Return to Player) values
    /// </summary>
    public static class RtpExtensions
    {
        private const int Divisor = 100;

        /// <summary>
        ///     Converts the rtp value to 64-bit signed integer that complies with meter requirements.
        ///     A value of 91.291 will be returned as 9129.
        /// </summary>
        /// <param name="this">The Rtp value</param>
        /// <returns>a 64-bit signed integer</returns>
        public static long ToMeter(this decimal @this)
        {
            return (long)(decimal.Round(@this, 2) * Divisor);
        }

        /// <summary>
        ///     Converts the rtp value from a 64-bit signed integer that complies with meter requirements. 
        /// </summary>
        /// <param name="this">a 64-bit signed integer</param>
        /// <returns>the RTP value</returns>
        public static decimal FromMeter(this long @this)
        {
            return decimal.Divide(@this, Divisor);
        }

        /// <summary>
        ///     Converts the rtp value to fraction
        /// </summary>
        /// <param name="this">The Rtp value</param>
        /// <returns></returns>
        public static decimal ToFraction(this decimal @this)
        {
            return decimal.Divide(@this, Divisor);
        }

        /// <summary>
        ///     Convert RTP percentage to displayable text. RTP values displayed on the UI are rounded up to 2 decimal places as
        ///     per Product Team.
        /// </summary>
        /// <param name="this">The Rtp percentage</param>
        /// <returns>RTP string formatted with the correct number of decimal place.</returns>
        public static string GetRtpString(this decimal @this)
        {
            var roundedRtp = Math.Round(
                @this,
                GamingConstants.NumberOfDecimalPlacesForRtpDisplay,
                MidpointRounding.AwayFromZero);

            return $"{roundedRtp:F2}%";
        }
    }
}