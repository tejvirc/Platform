namespace Aristocrat.Monaco.Sas
{
    using System;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Contracts.Metering;

    /// <summary>
    ///     A set of <see cref="IMeter" /> extensions
    /// </summary>
    public static class MeterExtensions
    {
        /// <summary>
        ///     Gets the meter results for the provided meter
        /// </summary>
        /// <param name="this">The meter you want to get the meter results for</param>
        /// <param name="accountingDenom">The accounting denom to use</param>
        /// <param name="meterType">The meter type that is for this meter</param>
        /// <param name="category">The meter category that is for this meter</param>
        /// <returns>The meter results for the provided meter</returns>
        public static MeterResult GetMeterResult(
            this IMeter @this,
            long accountingDenom,
            MeterType meterType,
            MeterCategory category)
        {
            var response = meterType == MeterType.Lifetime ? @this.Lifetime : @this.Period;
            switch (category)
            {
                case MeterCategory.Credit:
                    return new MeterResult(
                        response.MillicentsToAccountCredits(accountingDenom),
                        @this.Classification.GetMeterLength(accountingDenom, category));
                case MeterCategory.Cents:
                    return new MeterResult(
                        response.MillicentsToCents(),
                        @this.Classification.GetMeterLength(accountingDenom, category));
                case MeterCategory.Dollars:
                    return new MeterResult(
                        (long)Math.Floor(response.MillicentsToDollars()),
                        @this.Classification.GetMeterLength(accountingDenom, category));
                default:
                    return new MeterResult(response, @this.Classification.GetMeterLength(accountingDenom, category));
            }
        }
    }
}