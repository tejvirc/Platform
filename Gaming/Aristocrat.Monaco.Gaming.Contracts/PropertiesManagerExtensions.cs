namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Kernel;
    using Models;

    /// <summary>
    ///     Extension methods for the <see cref="IPropertiesManager" /> interface.
    /// </summary>
    public static class PropertiesManagerExtensions
    {
        static private readonly object _sync = new object();

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets the configured games.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <returns>An enumerator that allows foreach to be used to process the games in this collection.</returns>
        public static IEnumerable<IGameDetail> GetGames(this IPropertiesManager @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.GetValues<IGameDetail>(GamingConstants.Games);
        }

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets the specified game.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="gameId">Identifier for the game.</param>
        /// <returns>The specified game if found; otherwise null.</returns>
        public static IGameDetail GetGame(this IPropertiesManager @this, int gameId)
        {
            return @this.GetGames().FirstOrDefault(game => game.Id == gameId);
        }

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets the active game.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The active game if found; otherwise null.</returns>
        public static (IGameDetail game, IDenomination denomination) GetActiveGame(this IPropertiesManager @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            if (!@this.GetValue(GamingConstants.IsGameRunning, false))
            {
                return (null, null);
            }

            lock (_sync)
            {
                var game = @this.GetGame(@this.GetValue(GamingConstants.SelectedGameId, 0));
                var denom = game.Denominations.Single(d => d.Value == @this.GetValue(GamingConstants.SelectedDenom, 0L));
                return (game, denom);
            }
        }

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method to set the active game.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="gameId">The game type.</param>
        /// <param name="denomination">The game type.</param>
        /// <returns>The active game if found; otherwise null.</returns>
        //*** SetActiveGame
        public static void SetActiveGame(this IPropertiesManager @this, int gameId, long denomination)
        {
            lock (_sync)
            {
                @this.SetProperty(GamingConstants.SelectedGameId, gameId);
                @this.SetProperty(GamingConstants.SelectedDenom, denomination);
            }
        }

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets the selected game.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <returns>The active game if found; otherwise null.</returns>
        public static (IGameDetail game, IDenomination denomination) GetSelectedGame(this IPropertiesManager @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var game = @this.GetGame(@this.GetValue(GamingConstants.SelectedGameId, 0));
            var denom = game?.Denominations.Single(d => d.Value == @this.GetValue(GamingConstants.SelectedDenom, 0L));
            return (game, denom);
        }

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets the specified game.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="type">The game type.</param>
        /// <returns>The specified game if found; otherwise null.</returns>
        public static bool CanIncludeIncrementRtp(this IPropertiesManager @this, GameType type)
        {
            return CanIncludeSapIncrementRtp(@this, type);// Not supporting LP: || CanIncludeLinkProgressiveIncrementRtp(@this, type);
        }

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets the specified game.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="type">The game type.</param>
        /// <returns>The specified game if found; otherwise null.</returns>
        public static bool CanIncludeSapIncrementRtp(this IPropertiesManager @this, GameType type)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            switch (type)
            {
                case GameType.Slot:
                    return @this.GetValue(
                        GamingConstants.SlotsIncludeStandaloneProgressiveIncrementRtp,
                        false);
                case GameType.Poker:
                    return @this.GetValue(
                        GamingConstants.PokerIncludeStandaloneProgressiveIncrementRtp,
                        false);
                case GameType.Keno:
                    return @this.GetValue(
                        GamingConstants.KenoIncludeStandaloneProgressiveIncrementRtp,
                        false);
                case GameType.Blackjack:
                    return @this.GetValue(
                        GamingConstants.BlackjackIncludeStandaloneProgressiveIncrementRtp,
                        false);
                case GameType.Roulette:
                    return @this.GetValue(
                        GamingConstants.RouletteIncludeStandaloneProgressiveIncrementRtp,
                        false);
                default: return false;
            }
        }

        /// <summary>
        ///     An <see cref="IPropertiesManager" /> extension method that gets the specified game.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <param name="this">The @this to act on.</param>
        /// <param name="type">The game type.</param>
        /// <returns>The specified game if found; otherwise null.</returns>
        public static bool CanIncludeLinkProgressiveIncrementRtp(this IPropertiesManager @this, GameType type)
        {
            switch (type)
            {
                case GameType.Slot:
                    return @this.GetValue(GamingConstants.SlotsIncludeLinkProgressiveIncrementRtp, false);
                case GameType.Poker:
                    return @this.GetValue(GamingConstants.PokerIncludeLinkProgressiveIncrementRtp, false);
                case GameType.Keno:
                    return @this.GetValue(GamingConstants.KenoIncludeLinkProgressiveIncrementRtp, false);
                case GameType.Blackjack:
                    return @this.GetValue(GamingConstants.BlackjackIncludeLinkProgressiveIncrementRtp, false);
                case GameType.Roulette:
                    return @this.GetValue(GamingConstants.RouletteIncludeLinkProgressiveIncrementRtp, false);
                default: return false;
            }
        }
    }
}