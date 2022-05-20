namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Application.Contracts.Extensions;
    using PackageManifest.Models;

    /// <summary>
    ///     Extension methods for LevelDetail class
    /// </summary>
    public static class LevelDetailExtensions
    {
        /// <summary>
        ///     Calculates the reset value of a progressive level
        /// </summary>
        /// <param name="this">The progressive level information</param>
        /// <param name="denoms">The denoms for progressive level</param>
        /// <param name="betOption">The specific bet option for progressive level (null means all)</param>
        /// <returns>The reset value of progressive level in currency</returns>
        public static long ResetValue(this LevelDetail @this, IEnumerable<long> denoms, BetOption betOption)
        {
            if (@this.StartupValue.IsCredit && (denoms.Count() != 1 || betOption is null))
            {
                throw new InvalidProgressiveException($"Unable to determine ResetValue for ${@this.Name}.");
            }

            return ResetValue(@this.StartupValue, denoms.FirstOrDefault(), betOption);
        }

        /// <summary>
        ///     Calculates the maximum value of a progressive level
        /// </summary>
        /// <param name="this">The progressive level information</param>
        /// <param name="denoms">The denoms for progressive level</param>
        /// <returns>The maximum value of progressive level in currency</returns>
        public static long MaximumValue(this LevelDetail @this, IEnumerable<long> denoms)
        {
            if (@this.MaximumValue.IsCredit && (denoms.Count() != 1 ))
            {
                throw new InvalidProgressiveException($"Unable to determine MaximumValue for ${@this.Name}.");
            }
            return MaximumValue(@this.MaximumValue, denoms.FirstOrDefault());
        }

        /// <summary>
        ///     Calculates the reset value of a progressive level
        /// </summary>
        /// <param name="startupValue">The progressive startup value </param>
        /// <param name="denom">The denom for progressive level</param>
        /// <param name="betOption">The specific bet option for progressive level (null means all)</param>
        /// <returns>The reset value of progressive level in currency</returns>
        public static long ResetValue(ProgressiveValue startupValue, long denom, BetOption betOption)
        {
            var betMultiplier = betOption?.Bets.Max(b => b.Multiplier) ?? 1;
            return ValueInCents(startupValue, denom) * betMultiplier;
        }

        /// <summary>
        ///     Calculates the maximum value of a progressive level
        /// </summary>
        /// <param name="maxValue">The progressive maximum (ceiling) value</param>
        /// <param name="denom">The denom for progressive level</param>
        /// <returns>The maximum value of progressive level in currency</returns>
        public static long MaximumValue(ProgressiveValue maxValue, long denom)
        {
            return ValueInCents(maxValue, denom);
        }

        private static long ValueInCents(ProgressiveValue value, long denom)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value.ToCurrency(denom.MillicentsToCents());
        }
    }
}
