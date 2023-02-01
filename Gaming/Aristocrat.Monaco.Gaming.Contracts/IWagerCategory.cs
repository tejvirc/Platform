﻿namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     A wager category is a subset of a paytable with a specific payback percentage. A paytable may be made up of one or
    ///     more wager categories. Wager categories may be based on the amount wagered and/or the type of wager. Some paytables vary the theoretical payback percentage of the game based upon wager. These variants are identified
    ///     as wager categories.
    /// </summary>
    public interface IWagerCategory
    {
        /// <summary>
        ///     Gets the identifier of the wager category.
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Gets the theo payback pct.
        /// </summary>
        decimal TheoPaybackPercent { get; }

        /// <summary>
        ///     Gets the minimum wager credits.
        /// </summary>
        int? MinWagerCredits { get; }

        /// <summary>
        ///     Gets the maximum wager credits.
        /// </summary>
        int? MaxWagerCredits { get; }

        /// <summary>
        ///     Gets the maximum win amount.
        /// </summary>
        long MaxWinAmount { get; }

        /// <summary>
        ///     Gets or sets the minimum base RTP in percent.
        /// </summary>
        public decimal MinBaseRtpPercent { get; }

        /// <summary>
        ///     Gets or sets the maximum base RTP in percent.
        /// </summary>
        public decimal MaxBaseRtpPercent { get; }

        /// <summary>
        ///     Gets or sets the minimum SAP startup RTP in percent.
        /// </summary>
        public decimal MinSapStartupRtpPercent { get; }

        /// <summary>
        ///     Gets or sets the maximum SAP startup RTP in percent.
        /// </summary>
        public decimal MaxSapStartupRtpPercent { get; }

        /// <summary>
        ///     Gets or sets the minimum SAP increment RTP in percent
        /// </summary>
        public decimal SapIncrementRtpPercent { get; }

        /// <summary>
        ///     Gets or sets the minimum Link progressive startup RTP in percent.
        /// </summary>
        public decimal MinLinkStartupRtpPercent { get; }

        /// <summary>
        ///     Gets or sets the maximum Link progressive startup RTP in percent.
        /// </summary>
        public decimal MaxLinkStartupRtpPercent { get; }

        /// <summary>
        ///     Gets or sets the Link progressive increment RTP in percent
        /// </summary>
        public decimal LinkIncrementRtpPercent { get; }
    }
}