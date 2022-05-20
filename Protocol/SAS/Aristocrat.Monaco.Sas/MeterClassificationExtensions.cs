namespace Aristocrat.Monaco.Sas
{
    using System;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Contracts.Metering;

    /// <summary>
    ///     A set of <see cref="MeterClassification" /> extensions
    /// </summary>
    public static class MeterClassificationExtensions
    {
        /// <summary>
        ///     Gets the length of the provided meter classification
        /// </summary>
        /// <param name="this">The meter classification to get the length for</param>
        /// <param name="accountingDenom">The accounting denom to used</param>
        /// <param name="category">The meter category type that for this meter classification</param>
        /// <returns>The maximum length of the requested meter</returns>
        public static int GetMeterLength(this MeterClassification @this, long accountingDenom, MeterCategory category)
        {
            switch (category)
            {
                case MeterCategory.Credit:
                    return (int)Math.Floor(
                        Math.Log10((@this.UpperBounds - 1).MillicentsToAccountCredits(accountingDenom)) + 1);
                case MeterCategory.Cents:
                    return (int)Math.Floor(Math.Log10((@this.UpperBounds - 1).MillicentsToCents()) + 1);
                case MeterCategory.Dollars:
                    return (int)Math.Floor(
                        Math.Log10(decimal.ToDouble((@this.UpperBounds - 1).MillicentsToDollars()) + 1));
                default:
                    return (int)Math.Floor(Math.Log10(@this.UpperBounds - 1) + 1);
            }
        }
    }
}