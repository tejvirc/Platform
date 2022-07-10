namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;
    using Aristocrat.PackageManifest.Extension.v100;
    using Models;
    using PackageManifest.Models;

    /// <summary>
    ///     Defines a service for RTP-related logic.
    /// </summary>
    public interface IGameRtpService
    {
        /// <summary>
        ///     Return whether or not total RTP includes progressive increment contribution.
        /// </summary>
        /// <param name="type">Game type</param>
        /// <returns>Whether or not total RTP includes progressive increment contribution.</returns>
        bool CanIncludeIncrementRtp(GameType type);

        /// <summary>
        ///     Return whether or not total RTP includes standalone progressive increment contribution.
        /// </summary>
        /// <param name="type">Game type</param>
        /// <returns>Whether or not total RTP includes standalone progressive increment contribution.</returns>
        bool CanIncludeSapIncrementRtp(GameType type);

        /// <summary>
        ///     Return whether or not total RTP includes linked progressive increment contribution.
        /// </summary>
        /// <param name="type">Game type</param>
        /// <returns>Whether or not total RTP includes linked progressive increment contribution.</returns>
        bool CanIncludeLinkProgressiveIncrementRtp(GameType type);

        /// <summary>
        ///     Return total RTP range, calculated per jurisdiction rules.
        /// </summary>
        /// <param name="game">Game</param>
        /// <param name="progressiveDetails">Progressive info</param>
        /// <returns>Total RTP range</returns>
        RtpRange GetTotalRtp(GameAttributes game, IReadOnlyCollection<ProgressiveDetail> progressiveDetails);

        /// <summary>
        ///     Return whether or not total RTP is valid
        /// </summary>
        /// <param name="gameType">Game type</param>
        /// <param name="rtpRange">RTP range</param>
        /// <returns>Whether or not total RTP range is valid</returns>
        bool IsValidRtp(GameType gameType, RtpRange rtpRange);

        /// <summary>
        ///     Convert manifest game type to standard game type
        /// </summary>
        /// <param name="type">manifest game type</param>
        /// <returns>standard game type</returns>
        GameType ToGameType(t_gameType type);
    }
}
