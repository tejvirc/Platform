namespace Aristocrat.Monaco.Gaming.Contracts
{
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
        ///     Converts the rtp value to 64-bit signed integer
        /// </summary>
        /// <param name="this">The Rtp value</param>
        /// <returns></returns>
        public static decimal ToDecimal(this decimal @this)
        {
            return decimal.Divide(@this, Divisor);
        }
    }
}