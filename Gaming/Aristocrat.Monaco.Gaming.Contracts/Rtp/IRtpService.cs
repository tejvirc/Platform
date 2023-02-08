﻿namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     The RTP service is the authority for all processing of raw RTP information. The RTP formula can be manipulated
    ///     based on jurisdictional rules. These rules can be configured in <c>Gaming.config.xml</c> jurisdictional
    ///     settings file.
    /// </summary>
    /// <remarks>
    ///     <p>
    ///         The <see cref="RtpReport" /> generated by this service, is composed of <see cref="RtpBreakdown" /> objects.
    ///         These represent a detailed breakdown of a final RTP Range. In addition, each <see cref="RtpBreakdown" />
    ///         instance contains a <see cref="RtpValidationResult" />. Here is where all validation information is stored for
    ///         the corresponding RTP information.
    ///     </p>
    ///     <p>
    ///         Responsibilities of <see cref="IRtpService" /> include:
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>The authority for all RTP calculations</description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     Loads jurisdictional parameters for customizing the RTP calculation on a per jurisdiction
    ///                     basis.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>RTP validation against jurisdictional rules</description>
    ///             </item>
    ///         </list>
    ///     </p>

    /// ---------------------------
    /// 
    /// 
    /// </remarks>
    public interface IRtpService
    {
        /// <summary>
        ///     Generates an RTP report, spanning the given set of <see cref="IGameProfile" /> games.
        /// </summary>
        /// <param name="gameVariations">The <see cref="IGameProfile" />s to generate an RTP report for.</param>
        /// <returns>An RTP Report for the given set of <see cref="IGameProfile" />s.</returns>
        public RtpReport GetRtpReport(params IGameProfile[] gameVariations);

        /// <summary>
        ///     Gets the RTP report for game theme.
        /// </summary>
        /// <param name="gameThemeId">The game theme identifier.</param>
        /// <returns>An RTP Report for the given <c>gameThemeId</c>.</returns>
        public RtpReport GetRtpReportForGameTheme(string gameThemeId);

        /// <summary>
        ///     Gets the RTP report for variation.
        /// </summary>
        /// <param name="gameThemeId">The game theme identifier.</param>
        /// <param name="variationId">The variation identifier.</param>
        /// <returns>An RTP Report for the given <c>variationId</c>.</returns>
        public RtpReport GetRtpReportForVariation(string gameThemeId, string variationId);

        /// <summary>
        ///     Gets the RTP report for wager category.
        /// </summary>
        /// <param name="gameThemeId">The game theme identifier.</param>
        /// <param name="variationId">The variation identifier.</param>
        /// <param name="wagerCategoryId">The wager category identifier.</param>
        /// <returns>An RTP Report for the given <c>wagerCategoryId</c>.</returns>
        public RtpReport GetRtpReportForWagerCategory(string gameThemeId, string variationId, string wagerCategoryId);
    }
}