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
    ///            
    ///         </list>
    ///     </para>
    /// </remarks>
    public interface IRtpService
    {
        /// <summary>
        ///     Gets the average RTP percent over a set of games (variations) and their WagerCategories.
        /// </summary>
        /// <param name="games">The games.</param>
        decimal GetAverageRtp(IEnumerable<IGameProfile> games);

        /// <summary>
        ///     Gets the average RTP percent for a game (variation) and its WagerCategories.
        /// </summary>
        /// <param name="game">The game.</param>
        decimal GetAverageRtp(IGameProfile game);

        /// <summary>
        ///     Gets the Total RTP for a set of games (variations). Total RTP is an RTP Range composed of the lowest minimum and highest maximum RTP
        ///     of all RTP values of all given games (variations).
        /// </summary>
        /// <param name="games">The games used to calculate Total RTP with.</param>
        RtpRange GetTotalRtp(IEnumerable<IGameProfile> games);

        /// <summary>
        ///     Gets the total RTP for a single game (variation). Total RTP is an RTP Range composed of the lowest minimum and highest maximum RTP
        ///     of all RTP values of all given games (variations).
        /// </summary>
        /// <param name="game">The game used to calculate Total RTP with.</param>
        RtpRange GetTotalRtp(IGameProfile game);

        /// <summary>
        ///     Gets a Totaled RTP contribution breakdown for all Wager categories in the provided game (variation).
        /// </summary>
        /// <param name="game">The game (variations) for which to breakdown.</param>
        RtpBreakdown GetTotalRtpBreakdown(IGameProfile game);

        /// <summary>
        ///     Validates that all RTP values, contained in a game, conforms to jurisdictional and
        ///     business rules.
        /// </summary>
        /// <param name="game">The game to run RTP validation on.</param>
        RtpValidationReport ValidateGame(IGameProfile game);

        /// <summary>
        ///     Gets the jurisdiction RTP rules for a specific GameType.
        /// </summary>
        /// <param name="gameType">GameType.</param>
        RtpRules GetJurisdictionalRtpRules(GameType gameType);
    }
}