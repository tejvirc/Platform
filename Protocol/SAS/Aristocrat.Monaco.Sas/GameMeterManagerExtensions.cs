namespace Aristocrat.Monaco.Sas
{
    using Aristocrat.Sas.Client;
    using Contracts.Metering;
    using Gaming.Contracts.Meters;

    /// <summary>
    ///     Extension methods for the IGameMeterManager
    /// </summary>
    public static class GameMeterManagerExtensions
    {
        /// <summary>
        ///     Gets the meter value for the provided sas meter
        /// </summary>
        /// <param name="this">The game meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="meterId">The meter id to get the value for</param>
        /// <param name="meterType">The type of meter to retrieve</param>
        /// <param name="denom">The denom to use for the requested meter</param>
        /// <returns>The resulting meter value</returns>
        public static long GetMeterValue(
            this IGameMeterManager @this,
            long accountingDenom,
            SasMeters meterId,
            MeterType meterType,
            long denom)
        {
            var meterConfiguration = SasMeterNumberToMeterConfiguration.GetMeterConfiguration(meterId);
            return GetMeterValue(
                @this,
                accountingDenom,
                meterConfiguration.MeterName,
                denom,
                meterType,
                meterConfiguration.MeterCategory)?.MeterValue ?? 0L;
        }

        /// <summary>
        ///     Gets the meter value for the provided sas meter
        /// </summary>
        /// <param name="this">The game meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="meterId">The meter id to get the value for</param>
        /// <param name="meterType">The type of meter to retrieve</param>
        /// <param name="gameId">The sas meter for a game id</param>
        /// <param name="denom">The denom to use for the requested meter</param>
        /// <returns>The resulting meter value</returns>
        public static long GetMeterValue(
            this IGameMeterManager @this,
            long accountingDenom,
            SasMeters meterId,
            MeterType meterType,
            int gameId,
            long denom)
        {
            var meterConfiguration = SasMeterNumberToMeterConfiguration.GetMeterConfiguration(meterId);
            return GetMeterValue(
                @this,
                accountingDenom,
                meterConfiguration.MeterName,
                denom,
                gameId,
                meterType,
                meterConfiguration.MeterCategory)?.MeterValue ?? 0L;
        }

        /// <summary>
        ///     Gets the meter value for the provided sas meter
        /// </summary>
        /// <param name="this">The game meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="meterId">The meter id to get the value for</param>
        /// <param name="meterType">The type of meter to retrieve</param>
        /// <returns>The resulted meter value or null if no meter is found</returns>
        public static long GetMeterValue(
            this IGameMeterManager @this,
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
        ///     Gets the meter value for the provided sas meter from Table C-7 for given denom
        /// </summary>
        /// <param name="this">The game meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="sasMeter">The sas meter to get the value for</param>
        /// <returns>The resulted meter value or null if no meter is found</returns>
        public static MeterResult GetMeterValue(
            this IGameMeterManager @this,
            long accountingDenom,
            SasMeter sasMeter)
        {
            if (sasMeter == null)
            {
                return null;
            }

            var meterType = sasMeter.PeriodMeter ? MeterType.Period : MeterType.Lifetime;
            return GetMeterValue(
                @this,
                accountingDenom,
                sasMeter.MappedMeterName,
                meterType,
                sasMeter.Category);
        }

        /// <summary>
        ///     Gets the meter value for the provided sas meter from Table C-7 for given denom
        /// </summary>
        /// <param name="this">The game meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="sasMeter">The sas meter to get the value for</param>
        /// <param name="denom">The denom to use for the requested meter</param>
        /// <returns>The resulted meter value or null if no meter is found</returns>
        public static MeterResult GetMeterValue(
            this IGameMeterManager @this,
            long accountingDenom,
            SasMeter sasMeter,
            long denom)
        {
            if (sasMeter == null)
            {
                return null;
            }

            var meterType = sasMeter.PeriodMeter ? MeterType.Period : MeterType.Lifetime;
            return GetMeterValue(
                @this,
                accountingDenom,
                sasMeter.MappedMeterName,
                denom,
                meterType,
                sasMeter.Category);
        }

        /// <summary>
        ///     Gets the meter value for the provided sas meter from Table C-7 for given denom and game id
        /// </summary>
        /// <param name="this">The game meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="sasMeter">The sas meter to get the value for</param>
        /// <param name="denom">The denom to use for the requested meter</param>
        /// <param name="gameId">The sas meter for a game id</param>
        /// <returns>The resulted meter value or null if no meter is found</returns>
        public static MeterResult GetMeterValue(
            this IGameMeterManager @this,
            long accountingDenom,
            SasMeter sasMeter,
            long denom,
            int gameId)
        {
            if (sasMeter == null)
            {
                return null;
            }

            var meterType = sasMeter.PeriodMeter ? MeterType.Period : MeterType.Lifetime;
            return GetMeterValue(
                @this,
                accountingDenom,
                sasMeter.MappedMeterName,
                denom,
                gameId,
                meterType,
                sasMeter.Category);
        }

        /// <summary>
        ///     Gets the meter value for the provided sas meter from Table C-7 for given game id
        /// </summary>
        /// <param name="this">The game meter manager to use</param>
        /// <param name="accountingDenom">The accounting denom to be used</param>
        /// <param name="sasMeter">The sas meter to get the value for</param>
        /// <param name="gameId">The sas meter for a game id</param>
        /// <returns>The resulted meter value or null if no meter is found</returns>
        public static MeterResult GetMeterValue(
            this IGameMeterManager @this,
            long accountingDenom,
            SasMeter sasMeter,
            int gameId)
        {
            if (sasMeter == null)
            {
                return null;
            }

            var meterType = sasMeter.PeriodMeter ? MeterType.Period : MeterType.Lifetime;
            return GetMeterValue(
                @this,
                accountingDenom,
                sasMeter.MappedMeterName,
                gameId,
                meterType,
                sasMeter.Category);
        }

        private static MeterResult GetMeterValue(
            IGameMeterManager @this,
            long accountingDenom,
            string meterName,
            MeterType meterType,
            MeterCategory category)
        {
            return @this.IsMeterProvided(meterName)
                ? @this.GetMeter(meterName).GetMeterResult(accountingDenom, meterType, category)
                : null;
        }

        private static MeterResult GetMeterValue(
            IGameMeterManager @this,
            long accountingDenom,
            string meterName,
            long denom,
            MeterType meterType,
            MeterCategory category)
        {
            return @this.IsMeterProvided(denom, meterName)
                ? @this.GetMeter(denom, meterName).GetMeterResult(accountingDenom, meterType, category)
                : null;
        }

        private static MeterResult GetMeterValue(
            IGameMeterManager @this,
            long accountingDenom,
            string meterName,
            long denom,
            int gameId,
            MeterType meterType,
            MeterCategory category)
        {
            return @this.IsMeterProvided(gameId, denom, meterName)
                ? @this.GetMeter(gameId, denom, meterName).GetMeterResult(accountingDenom, meterType, category)
                : null;
        }

        private static MeterResult GetMeterValue(
            IGameMeterManager @this,
            long accountingDenom,
            string meterName,
            int gameId,
            MeterType meterType,
            MeterCategory category)
        {
            return @this.IsMeterProvided(gameId, meterName)
                ? @this.GetMeter(gameId, meterName).GetMeterResult(accountingDenom, meterType, category)
                : null;
        }
    }
}