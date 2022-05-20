namespace Aristocrat.Monaco.Application.Contracts.Metering
{
    using System;

    /// <summary>
    ///     Definition of the MeterUtilities class
    /// </summary>
    public static class MeterUtilities
    {
        /// <summary>
        ///     Parse the meter classification
        /// </summary>
        /// <param name="classification">The classification of the meter</param>
        /// <returns>The type of meter classification</returns>
        internal static MeterClassification ParseClassification(string classification)
        {
            MeterClassification type;
            switch (classification)
            {
                case "Occurrence":
                    type = new OccurrenceMeterClassification();
                    break;
                case "Currency":
                    type = new CurrencyMeterClassification();
                    break;
                case "Percentage":
                    type = new PercentageMeterClassification();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown meter classification {classification}");
            }

            return type;
        }

        /// <summary>
        ///     Gets the meter value based on the lifetime
        /// </summary>
        /// <param name="this">The meter instance</param>
        /// <param name="timeFrame">The meter time frame</param>
        /// <returns>The meter value</returns>
        public static long GetValue(this IMeter @this, MeterTimeframe timeFrame)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            switch (timeFrame)
            {
                case MeterTimeframe.Lifetime:
                    return @this.Lifetime;
                case MeterTimeframe.Period:
                    return @this.Period;
                case MeterTimeframe.Session:
                    return @this.Session;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeFrame), timeFrame, null);
            }
        }
    }
}