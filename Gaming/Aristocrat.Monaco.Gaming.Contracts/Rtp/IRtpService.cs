namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    using System.Collections.Generic;
    using Models;

    /// <summary>
    ///     An interface by which all ReturnToPlayer (RTP) information for games can queried and validated in once centralized
    ///     service.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This service does not cache any RTP values or game state. Instead it acts as a set of helper methods for
    ///         working with RTP information, while managing RTP calculation and RTP rules under the covers.
    ///     </para>
    ///     <para>
    ///         The major roles of this interface are to provide a means for:
    ///         <list type="bullet">
    ///             <item>
    ///                 Querying for information about how RTP values are broken down into fine-grain
    ///                 contributions from various sources. For example: Base Game RTP, Progressive increment RTP, etc.
    ///             </item>
    ///             <item>
    ///                 Querying the currently loaded RTP rules.
    ///             </item>
    ///             <item>
    ///                 Calculating Total RTP over any set of games and WagerCategories
    ///             </item>
    ///             <item>
    ///                 ICalculating Average RTP over any set of games and WagerCategories.
    ///             </item>
    ///             <item>
    ///                 Automatically loading and utilizing RTP rules from jurisdictional configs to customize/restrict the RTP
    ///                 contribution calculations.
    ///             </item>
    ///             <item>
    ///                 Validating that RTP information meets specific requirements set by regulators and business
    ///                 management.
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public interface IRtpService
    {
        /// <summary>
        ///     Gets an object containing a breakdown of the Total RTP of a game's WagerCategory, into its individual
        ///     contributions.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         Important information about RTP validation:
        ///         <list type="bullet">
        ///             <item>
        ///                 This method validates all RTP contributions represented in the breakdown and verifies progressive RTP
        ///                 with the progressive provider. Validation results can be reviewed by checking the
        ///                 <see cref="RtpBreakdown.IsValid" /> and <see cref="RtpBreakdown.FailureFlags" /> properties of the
        ///                 <see cref="RtpBreakdown" /> object that's returned.
        ///             </item>
        ///             <item>
        ///                 This service does not disable games with invalid RTP or modify the game models in any way. Invalid
        ///                 games will need to be handled externally, most likely disabled or taken out of play, according to the
        ///                 business logic. The reason for this service not disabling games, is to help maintain a stateless
        ///                 service with no destructive actions.
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        /// <param name="game">The <see cref="IGameDetail" /> to retrieve the RTP breakdown from.</param>
        /// <param name="wagerCategoryId">The id of the wager category used to populate the <see cref="RtpBreakdown" /> fields.</param>
        /// <returns>A breakdown of the Total RTP for the given game, into its individual contributions.</returns>
        RtpBreakdown GetRtpBreakdownForWagerCategory(IGameDetail game, string wagerCategoryId);

        /// <summary>
        ///     Gets a Totaled RTP contribution breakdown for all Wager categories for the provided game (variation).
        /// </summary>
        /// <param name="game">The game (variations) for which to breakdown.</param>
        /// <returns>The total RTP Breakdown for the game.</returns>
        RtpBreakdown GetTotalRtpBreakdown(IGameDetail game);

        /// <summary>
        ///     Gets the Total RTP across a set of game variations. Total RTP is an RTP Range composed of the lowest minimum and
        ///     highest maximum RTP of all RTP values in the given games (variations).
        /// </summary>
        /// <param name="game">The game to calculate Total RTP.</param>
        /// <returns>The total RTP for the game.</returns>
        RtpRange GetTotalRtp(IGameDetail game);

        /// <summary>
        ///     Gets the Total RTP across a set of game variations. Total RTP is an RTP Range composed of the lowest minimum and
        ///     highest maximum RTP of all RTP values in the given games (variations).
        /// </summary>
        /// <param name="games">The games to calculate Total RTP over.</param>
        /// <returns></returns>
        RtpRange GetTotalRtp(IEnumerable<IGameDetail> games);

        /// <summary>
        ///     Gets the total Return to Player payback percent from all SubGames.
        /// </summary>
        /// <param name="subGames">The sub games to get Total RTP for.</param>
        /// <returns>The total RTP range over the set of given SubGames</returns>
        RtpRange GetTotalSubGameRtp(IEnumerable<ISubGameDetails> subGames);

        /// <summary>
        ///     Gets the SubGame Return to Player payback percent.
        /// </summary>
        /// <param name="subGame">The sub game to get the RTP for.</param>
        /// <returns>The SubGame RTP</returns>
        RtpRange GetSubGameRtp(ISubGameDetails subGame);

        /// <summary>
        ///     Gets the average RTP percent over the given set of games (variations) and their WagerCategories.
        /// </summary>
        /// <param name="game">The game to calculate Average RTP for.</param>
        /// <returns>The games average RTP</returns>
        decimal GetAverageRtp(IGameDetail game);

        /// <summary>
        ///     Gets the average RTP percent over the given set of games (variations) and their WagerCategories.
        /// </summary>
        /// <param name="games">The list of games to calculate Average RTP for.</param>
        /// <returns>The average RTP over the set of games</returns>
        decimal GetAverageRtp(IEnumerable<IGameDetail> games);

        /// <summary>
        ///     Gets the jurisdiction RTP rules for a specific GameType.
        /// </summary>
        /// <param name="gameType">The GameType to get the rules for.</param>
        /// <returns>The jurisdictional rules</returns>
        RtpRules GetJurisdictionalRtpRules(GameType gameType);
    }
}