namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using System;

    /// <summary>
    ///     A set of extensions for percentages
    /// </summary>
    public static class PercentExtensions
    {
        /// <summary>
        ///     A decimal extension method that converts the @this to a percentage.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a long.</returns>
        public static long ToPercentage(this decimal @this)
        {
            return Convert.ToInt64(@this * GamingConstants.PercentageConversion);
        }

        /// <summary>
        ///     A decimal extension method that converts the @this to a percentage.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as a long.</returns>
        public static decimal ToPercentage(this long @this)
        {
            return (decimal)@this / GamingConstants.PercentageConversion;
        }

        /// <summary>
        ///     A decimal extension method that converts the @this (probability) to the odds.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <returns>@this as an int.</returns>
        public static int ToOdds(this decimal @this)
        {
            return @this != 0M ? Convert.ToInt32(1M / @this) : 0;
        }
    }
}