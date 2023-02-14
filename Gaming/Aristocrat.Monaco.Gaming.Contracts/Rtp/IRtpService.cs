namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System.Collections.Generic;
    using Models;

    /// <summary>
    ///     TODO: Edit XML Comment
    ///
    ///
    /// 
    /// </summary>
    public interface IRtpService
    {
        /// <summary>
        ///     Gets the average RTP.
        /// </summary>
        /// <param name="games">The games.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetAverageRtp
        decimal GetAverageRtp(IEnumerable<IGameProfile> games);

        /// <summary>
        ///     Gets the total RTP for a set of games.
        /// </summary>
        /// <param name="games">The games.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetTotalRtp
        RtpRange GetTotalRtp(IEnumerable<IGameProfile> games);

        /// <summary>
        ///     Gets the total RTP for a game.
        /// </summary>
        /// <param name="game">The game.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetTotalRtp
        RtpRange GetTotalRtp(IGameProfile game);

        /// <summary>
        ///     Gets the RTP breakdown for a specific WagerCategory.
        /// </summary>
        /// <param name="wagerCategoryId">The wager category identifier.</param>
        /// <param name="game">The game.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetRtpBreakdown
        RtpBreakdown GetRtpBreakdown(IGameProfile game, string wagerCategoryId);

        /// <summary>
        ///     Gets a totaled RTP breakdown for all wager categories. 
        /// </summary>
        /// <param name="game">The game.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetRtpBreakdown
        RtpBreakdown GetTotalRtpBreakdown(IGameProfile game);

        /// <summary>
        ///     Validates the RTP.
        /// </summary>
        /// <param name="games">The games.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetValidationReport
        RtpValidationReport GetValidationReport(IEnumerable<IGameProfile> games);

        /// <summary>
        ///     Gets the jurisdiction RTP rules.
        /// </summary>
        /// <param name="gameType">Type of the game.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetJurisdictionalRtpRules
        RtpRules GetJurisdictionalRtpRules(GameType gameType);
    }
}