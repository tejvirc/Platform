namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Numerics;

    /// <summary>
    ///     An extension class for helping residual calculations.
    /// </summary>
    public static class ResidualExtensions
    {
        /// <summary>
        ///     A <see cref="BigInteger"/> extension method that converts a value to a payable.
        /// </summary>
        /// <param name="value">The value to act on.</param>
        /// <returns>Value as a long in payable units.</returns>
        public static long ToPayable(this BigInteger value)
        {
            return (long)(value / ResidualValue.TruncationFactor * ResidualValue.PayableUnit);
        }

        /// <summary>
        ///     A <see cref="BigInteger"/> extension method that converts a value to a payable.
        /// </summary>
        /// <param name="value">The value to act on.</param>
        /// <returns>Value as a long in payable units.</returns>
        public static long ToPayable(this long value)
        {
            return value / ResidualValue.PayableUnit * ResidualValue.PayableUnit;
        }
    }
}