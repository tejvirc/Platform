namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System.Collections.Generic;

    /// <summary>
    ///     Provides a mechanism to get the sub game details
    /// </summary>
    public interface ISubGameDetails : IGame
    {
        /// <summary>
        ///     Gets the maximum wager
        /// </summary>
        int MaximumWagerCredits { get; }

        /// <summary>
        ///     Gets the minimum wager
        /// </summary>
        int MinimumWagerCredits { get; }

        /// <summary>
        ///     Gets the maximum win amount
        /// </summary>
        long MaximumWinAmount { get; }

        /// <summary>
        ///     Gets the maximum theoretical payback percentage for the game; a value of 0 (zero) indicates that the attribute is
        ///     not supported; otherwise, MUST be set to the maximum payback percentage of the game, which MUST be greater than 0
        ///     (zero). For example, a value of 96.37777 represents a maximum payback percentage of 96.37777%
        /// </summary>
        decimal MaximumPaybackPercent { get; }

        /// <summary>
        ///     Gets the minimum theoretical payback percentage for the game; a value of 0 (zero) indicates that the attribute is
        ///     not supported; otherwise, MUST be set to the minimum payback percentage for the game, which MUST be greater than 0
        ///     (zero). For example, a value of 82.45555 represent a minimum theoretical payback of 82.45555%
        /// </summary>
        decimal MinimumPaybackPercent { get; }

        /// <summary>
        ///     Gets the paytable description
        /// </summary>
        string PaytableName { get; }

        /// <summary>
        ///     Gets the CdsThemeId
        /// </summary>
        string CdsThemeId { get; }

        /// <summary>
        ///     Gets the CdsTitleId
        /// </summary>
        string CdsTitleId { get; }

        /// <summary>
        ///     Gets the list of all denominations
        /// </summary>
        IEnumerable<IDenomination> Denominations { get; }

        /// <summary>
        ///     Gets a list of the active denominations
        /// </summary>
        IEnumerable<long> ActiveDenoms { get; }

        /// <summary>
        ///     Gets a list of the supported denominations
        /// </summary>
        IEnumerable<long> SupportedDenoms { get; }

        /// <summary>
        ///     Gets a list of the Cds Game Information
        /// </summary>
        IEnumerable<ICdsGameInfo> CdsGameInfos { get; }
    }
}