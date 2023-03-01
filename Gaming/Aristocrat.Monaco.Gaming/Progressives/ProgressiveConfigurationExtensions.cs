namespace Aristocrat.Monaco.Gaming.Progressives
{
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     A set of <see cref="IProgressiveConfigurationProvider" /> extensions
    /// </summary>
    public static class ProgressiveConfigurationExtensions
    {
        /// <summary>
        ///      Gets the Progressive Pack Rtp information
        /// </summary>
        /// <param name="this">The progressive configuration provider</param>
        /// <param name="gameId">The game id</param>
        /// <param name="denomination">The denomination</param>
        /// <param name="betOption">The betOption</param>
        public static (ProgressiveRtp, RtpVerifiedState) GetProgressivePackRtp(
            this IProgressiveConfigurationProvider @this,
            int gameId,
            long denomination,
            string betOption)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var configuredLevels = @this.ViewAssignedProgressiveLevels(gameId, denomination, betOption);
            if (!configuredLevels.Any())
            {
                return (null, RtpVerifiedState.NotAvailable);
            }

            configuredLevels = configuredLevels.Where(level => level.LevelType == ProgressiveLevelType.Sap);

            return !configuredLevels.Any() ? (null, RtpVerifiedState.NotVerified) : (configuredLevels.First().ProgressivePackRtp, RtpVerifiedState.Verified);
        }

        /// <summary>
        ///      Gets the Progressive levels that have been assigned to the game/denom/betoption
        /// </summary>
        /// <param name="this">The progressive configuration provider</param>
        /// <param name="gameId">The game id</param>
        /// <param name="denom">The denomination</param>
        /// <param name="betOption">The betOption</param>
        private static IEnumerable<IViewableProgressiveLevel> ViewAssignedProgressiveLevels(
            this IProgressiveConfigurationProvider @this,
            int gameId,
            long denom,
            string betOption)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.ViewProgressiveLevels(gameId, denom)
                .Where(
                    level => (string.IsNullOrEmpty(level.BetOption) || level.BetOption == betOption) &&
                             (level.LevelType == ProgressiveLevelType.Sap /* always assigned*/ ||
                              // check all other levels (LP or Selectable to make sure level is assigned to a progressive)
                              (level.AssignedProgressiveId.AssignedProgressiveType != AssignableProgressiveType.None &&
                               !string.IsNullOrEmpty(level.AssignedProgressiveId.AssignedProgressiveKey))));
        }
    }
}

