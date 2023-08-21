namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System;
    using System.Linq;
    using Contracts.Progressives;

    /// <summary>
    ///     A set of extension methods for <see cref="IProgressiveConfigurationProvider" />
    /// </summary>
    public static class ProgressiveConfigurationProviderExtensions
    {
        /// <summary>
        ///     Gets an <see cref="IViewableProgressiveLevel" /> based on a <see cref="JackpotInfo"/>
        /// </summary>
        /// <param name="this">An <see cref="IProgressiveConfigurationProvider" /> instance</param>
        /// <param name="jackpotInfo">A <see cref="JackpotInfo"/></param>
        /// <param name="gameId">The game identifier</param>
        /// <param name="denomination">The denomination</param>
        /// <returns>A <see cref="IViewableProgressiveLevel"/> if found, otherwise null</returns>
        public static IViewableProgressiveLevel GetProgressiveLevel(this IProgressiveConfigurationProvider @this, JackpotInfo jackpotInfo, int gameId, long denomination)
        {
            if (jackpotInfo == null)
            {
                throw new ArgumentNullException(nameof(jackpotInfo));
            }

            return GetProgressiveLevel(@this, jackpotInfo.PackName, gameId, denomination, jackpotInfo.LevelId, jackpotInfo.WagerCredits);
        }

        /// <summary>
        ///     Gets an <see cref="IViewableProgressiveLevel" /> based on the specified values
        /// </summary>
        /// <param name="this">An <see cref="IProgressiveLevelProvider" /> instance</param>
        /// <param name="packName">The pack name</param>
        /// <param name="gameId">The game identifier</param>
        /// <param name="denomination">The denomination</param>
        /// <param name="levelId">The level Id</param>
        /// <param name="wagerCredits">Wager Credits for the level</param>
        /// <returns>A <see cref="IViewableProgressiveLevel"/> if found, otherwise null</returns>
        private static IViewableProgressiveLevel GetProgressiveLevel(
            this IProgressiveConfigurationProvider @this,
            string packName,
            int gameId,
            long denomination,
            int levelId,
            long wagerCredits)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.ViewProgressiveLevels(gameId, denomination, packName)
                .SingleOrDefault(l => l.LevelId == levelId && l.WagerCredits == wagerCredits);
        }
    }
}