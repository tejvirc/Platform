namespace Aristocrat.Monaco.Gaming.Contracts.Rtp
{
    /// <summary>
    ///     The RTP service is the authority for all processing of raw RTP information. The RTP formula can be calculated
    ///     differently based on jurisdictional rules. These rules can be configured in <c>Gaming.config.xml</c> jurisdictional
    ///     settings file.
    /// </summary>
    /// <remarks>
    ///     Responsibilities of <see cref="IRtpService" /> include:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Querying games for RTP information</description>
    ///         </item>
    ///         <item>
    ///             <description>RTP validation against jurisdictional rules</description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public interface IRtpService
    {
        /// <summary>
        ///     Generates a statistical report covering all variations of RTP for the given Game Theme
        /// </summary>
        /// <param name="gameThemeId">The game theme identifier.</param>
        /// <returns>The RTP report for the given game theme</returns>
        public GameThemeRtpReport GenerateRtpReportForGame(string gameThemeId);
    }
}