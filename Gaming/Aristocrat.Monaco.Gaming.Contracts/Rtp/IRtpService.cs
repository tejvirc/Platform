namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System.Collections.Generic;

    /// <summary>
    ///     TODO: Edit XML Comment 
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
        ///     Gets the total RTP.
        /// </summary>
        /// <param name="games">The games.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetTotalRtp
        RtpRange GetTotalRtp(IEnumerable<IGameProfile> games);

        /// <summary>
        ///     Gets the RTP breakdown.
        /// </summary>
        /// <param name="wagerCategoryId">The wager category identifier.</param>
        /// <param name="game">The game.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for GetRtpBreakdown
        RtpBreakdown GetRtpBreakdown(string wagerCategoryId, IGameProfile game);

        /// <summary>
        ///     Validates the RTP.
        /// </summary>
        /// <param name="games">The games.</param>
        /// <returns></returns>
        /// TODO Edit XML Comment Template for ValidateRtp
        RtpValidationReport ValidateRtp(IEnumerable<IGameProfile> games);
    }
}