namespace Aristocrat.Monaco.Sas
{
    using Application.Contracts;
    using Aristocrat.Sas.Client;
    using Contracts.Metering;

    /// <summary>
    ///     Extension methods for the IMeterManager
    /// </summary>
    public static class MeterManagerExtensions
    {
        /// <summary>
        ///     Gets the meter value for the provided sas meter
        /// </summary>
        /// <param name="this">The meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="meterId">The meter id to get the value for</param>
        /// <param name="meterType">The type of meter to retrieve</param>
        /// <returns>The resulting meter value. Zero is returned if no meter is found</returns>
        public static long GetMeterValue(
            this IMeterManager @this,
            long accountingDenom,
            SasMeters meterId,
            MeterType meterType)
        {
            var meterConfiguration = SasMeterNumberToMeterConfiguration.GetMeterConfiguration(meterId);
            return GetMeterValue(
                @this,
                accountingDenom,
                meterConfiguration.MeterName,
                meterType,
                meterConfiguration.MeterCategory)?.MeterValue ?? 0L;
        }

        /// <summary>
        ///     Gets the meter value for the provided sas meter from Table C-7
        /// </summary>
        /// <param name="this">The meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="sasMeter">The sas meter to get the value for</param>
        /// <returns>The resulted meter value or null if no meter is found</returns>
        public static MeterResult GetMeterValue(
            this IMeterManager @this,
            long accountingDenom,
            SasMeter sasMeter)
        {
            if (sasMeter == null)
            {
                return null;
            }

            var meterType = sasMeter.PeriodMeter ? MeterType.Period : MeterType.Lifetime;
            return GetMeterValue(@this, accountingDenom, sasMeter.MappedMeterName, meterType, sasMeter.Category);
        }

        private static MeterResult GetMeterValue(
            IMeterManager @this,
            long accountingDenom,
            string meterName,
            MeterType meterType,
            MeterCategory category)
        {
            return @this.IsMeterProvided(meterName)
                ? @this.GetMeter(meterName).GetMeterResult(accountingDenom, meterType, category)
                : null;
        }
    }
}