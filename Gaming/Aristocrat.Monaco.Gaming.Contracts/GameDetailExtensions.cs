namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Models;
    using PackageManifest.Models;

    /// <summary>
    ///     A set of <see cref="IGameDetail" /> extensions
    /// </summary>
    public static class GameDetailExtensions
    {
        /// <summary>
        ///     Returns the bet option for a game at a denomination
        /// </summary>
        /// <param name="this">The Game Detail</param>
        /// <param name="denom">The denomination</param>
        public static BetOption GetBetOption(this IGameDetail @this, long denom)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var denomination = @this.Denominations.Single(d => d.Value == denom);
            return @this.BetOptionList.FirstOrDefault(b => b.Name == denomination.BetOption);
        }

        /// <summary>
        ///     Returns the minimum wager credits for a game at a given bet option
        /// </summary>
        /// <param name="this">The Game Detail</param>
        /// <param name="betOption">The bet option</param>
        /// <param name="lineOption">The line option</param>
        public static int MinimumWagerCredits(this IGameDetail @this, BetOption betOption, LineOption lineOption)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var minBetMultiplier = betOption?.Bets.Min(b => b.Multiplier);
            if (minBetMultiplier is null)
            {
                return @this.MinimumWagerCredits;
            }

            return minBetMultiplier.Value * @this.BaseMinWagerCredits(lineOption);
        }

        /// <summary>
        ///     Returns the maximum wager credits for a game across all active denoms
        /// </summary>
        /// <param name="this">The Game Detail</param>
        public static int MaximumActiveWagerCredits(this IGameDetail @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (!@this.ActiveDenominations.Any())
            {
                return 0;
            }

            return @this.Denominations.Where(d => d.Active).Max(denomination=> @this.MaximumWagerCredits(
                @this.BetOptionList?.SingleOrDefault(b => b.Name == denomination.BetOption),
                @this.LineOptionList?.SingleOrDefault(l => l.Name == denomination.LineOption)));
        }

        /// <summary>
        ///     Returns the maximum wager credits for a game at a given denom
        /// </summary>
        /// <param name="this">The Game Detail</param>
        /// <param name="denomination">The denomination</param>
        public static int MaximumWagerCredits(this IGameDetail @this, IDenomination denomination)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }
            
            if (denomination is null)
            {
                throw new ArgumentNullException(nameof(denomination));
            }

            return @this.MaximumWagerCredits(
                    @this.BetOptionList?.SingleOrDefault(b => b.Name == denomination.BetOption),
                    @this.LineOptionList?.SingleOrDefault(l => l.Name == denomination.LineOption));
        }

        /// <summary>
        ///     Returns the maximum wager credits for a game at a given bet option
        /// </summary>
        /// <param name="this">The Game Detail</param>
        /// <param name="betOption">The bet option</param>
        /// <param name="lineOption">The line option</param>
        public static int MaximumWagerCredits(this IGameDetail @this, BetOption betOption, LineOption lineOption)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (betOption?.MaxInitialBet != null) // independent of any line option
            {
                return betOption.MaxInitialBet.Value;
            }

            var maxBetMultiplier = betOption?.Bets.MaxOrDefault(b => b.Multiplier, 1);
            if (maxBetMultiplier is null)
            {
                return @this.MaximumWagerCredits;
            }

            return maxBetMultiplier.Value * @this.BaseMaxWagerCredits(lineOption);
        }

        /// <summary>
        ///     Returns a list of all of the bets for a given game
        /// </summary>
        /// <param name="this">The Game Detail</param>
        /// <param name="betOption">The bet option</param>
        /// <param name="lineOption">The line option</param>
        /// <param name="denom">The denom in dollars for this game configuration</param>
        /// <param name="type">The type of game we want bet options for</param>
        /// <returns>The list of bets for the game</returns>
        public static IEnumerable<decimal> GetBetAmounts(this IGameDetail @this, BetOption betOption, LineOption lineOption, decimal denom, GameType type)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (betOption is null || lineOption is null)
            {
                var minBetCredits = @this.MinimumWagerCredits;
                var maxBetCredits = @this.MaximumWagerCredits;
                return Enumerable.Range(minBetCredits, (maxBetCredits - minBetCredits)+1).Select(x => x * denom);
            }

            if (betOption.MaxInitialBet != null)
            {
                var minMultiplier = betOption.Bets.Min(b => b.Multiplier);
                return Enumerable.Range(minMultiplier, (betOption.MaxInitialBet.Value - minMultiplier)+1)
                    .Select(m => m * denom);
            }

            var maxBet = betOption.Bets.Max(b => b.Multiplier);
            if (type != GameType.Poker)
            {
                return betOption.Bets.SelectMany(
                        b => lineOption.Lines.SelectMany(
                            l => Enumerable.Range(0, l.Multiplier)
                                .Select(multi => (multi * maxBet + b.Multiplier) * l.Cost * denom))).Distinct()
                    .OrderBy(x => x);
            }

            var minBet = betOption.Bets.Min(b => b.Multiplier);
            maxBet = betOption.Bets.Max(b => b.Multiplier) * lineOption.Lines.Max(l => l.Multiplier) * lineOption.Lines.Max(l => l.Cost);
            return Enumerable.Range(minBet, maxBet).Select(c => c * denom);
        }
        
        /// <summary>
        ///     Returns the maximum wager for a game at a given denomination
        /// </summary>
        /// <param name="this">The Game Detail</param>
        /// <param name="denomination">The denomination</param>
        public static long MaximumWager(this IGameDetail @this, IDenomination denomination)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (denomination == null)
            {
                throw new ArgumentNullException(nameof(denomination));
            }

            var betOption = @this.BetOptionList?.FirstOrDefault(b => b.Name == denomination.BetOption);
            var lineOption = @this.LineOptionList?.FirstOrDefault(l => l.Name == denomination.LineOption);

            return @this.MaximumWagerCredits(betOption, lineOption) * denomination.Value;
        }

        /// <summary>
        ///     Returns the top award for a game at a given denomination
        /// </summary>
        /// <param name="this">The Game Detail</param>
        /// <param name="denomination">The denomination</param>
        public static long TopAward(this IGameDetail @this, IDenomination denomination)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.TopAward(
                denomination,
                @this.BetOptionList?.FirstOrDefault(b => b.Name == denomination.BetOption),
                @this.LineOptionList?.FirstOrDefault(l => l.Name == denomination.LineOption));
        }

        /// <summary>
        ///     Returns the top award for a game at a given denomination, bet option, and line option
        /// </summary>
        /// <param name="this">The Game Detail</param>
        /// <param name="denomination">The denomination</param>
        /// <param name="betOption">The bet option</param>
        /// <param name="lineOption">The line option</param>
        public static long TopAward(this IGameDetail @this, IDenomination denomination, BetOption betOption, LineOption lineOption)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (denomination == null)
            {
                return 0L;
            }

            return @this.MaximumWagerCredits(betOption, lineOption) * @this.WinThreshold * denomination.Value;
        }

        private static int BaseMinWagerCredits(this IGameDetail @this, LineOption lineOption)
        {
            var lineCost = lineOption?.Lines.MinOrDefault(l => l.Cost, 0) ?? 0;
            if (lineCost > 0)
            {
                return lineCost;
            }

            var minBetMultiplier =
                @this.BetOptionList?.MinOrDefault(o => o.Bets.MinOrDefault(b => b.Multiplier, 0), 0) ?? 0;
            return minBetMultiplier <= 1 ? @this.MinimumWagerCredits : @this.MinimumWagerCredits / minBetMultiplier;
        }

        private static int BaseMaxWagerCredits(this IGameDetail @this, LineOption lineOption)
        {
            var lineCost = lineOption?.Lines.MaxOrDefault(l => l.TotalCost, 0) ?? 0;
            if (lineCost > 0)
            {
                return lineCost;
            }

            var maxBetMultiplier =
                @this.BetOptionList?.MaxOrDefault(o => o.Bets.MaxOrDefault(b => b.Multiplier, 0), 0) ?? 0;
            return maxBetMultiplier <= 1 ? @this.MaximumWagerCredits : @this.MaximumWagerCredits / maxBetMultiplier;
        }
    }
}
