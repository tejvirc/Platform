namespace Aristocrat.Monaco.Gaming.Contracts.Meters
{
    using System;

    /// <summary>
    ///     A set of <see cref="IGameMeterManager" /> extensions
    /// </summary>
    public static class GameMeterManagerExtensions
    {
        /// <summary>
        ///     Increments all games played related meters
        /// </summary>
        /// <param name="this">The Game Meter Manager</param>
        /// <param name="gameId">The unique game Id</param>
        /// <param name="denomId">The denomination Id</param>
        /// <param name="wagerCategory">The wager category</param>
        /// <param name="result">The game result</param>
        /// <param name="carded">true if there is an active player session</param>
        public static void IncrementGamesPlayed(
            this IGameMeterManager @this,
            int gameId,
            long denomId,
            IWagerCategory wagerCategory,
            GameResult result,
            bool carded)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            @this.GetMeter(gameId, denomId, GamingMeters.PlayedCount).Increment(1);
            if (carded)
            {
                @this.GetMeter(PlayerMeters.CardedPlayedCount).Increment(1);
            }

            @this.GetMeter(GamingMeters.GamesPlayedSinceDoorClosed).Increment(1);

            @this.GetMeter(GamingMeters.GamesPlayedSinceDoorOpen).Increment(1);

            @this.GetMeter(GamingMeters.GamesPlayedSinceReboot).Increment(1);

            switch (result)
            {
                case GameResult.Won:
                    @this.GetMeter(gameId, denomId, GamingMeters.WonCount).Increment(1);
                    break;
                case GameResult.Lost:
                    @this.GetMeter(gameId, denomId, GamingMeters.LostCount).Increment(1);
                    break;
                case GameResult.Tied:
                    @this.GetMeter(gameId, denomId, GamingMeters.TiedCount).Increment(1);
                    break;
            }

            if (wagerCategory != null)
            {
                @this.GetMeter(gameId, denomId, wagerCategory.Id, GamingMeters.WagerCategoryPlayedCount).Increment(1);
            }
        }
    }
}
