namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines a wager category
    /// </summary>
    public class WagerCategory
    {
        /// <summary>
        ///     Gets or sets the Wager category identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Gets or sets the theoretical payback percentage associated with the wager category
        /// </summary>
        public decimal TheoPaybackPercent { get; set; }

        /// <summary>
        ///     Gets or sets the minimum wager, in credits, associated with the wager category
        /// </summary>
        public int MinWagerCredits { get; set; }

        /// <summary>
        ///     Gets or sets the maximum wager, in credits, associated with the wager category
        /// </summary>
        public int MaxWagerCredits { get; set; }

        /// <summary>
        ///     Gets or sets the maximum win amount, in millicents, associated with the wager category
        /// </summary>
        public long MaxWinAmount { get; set; }

        /// <summary>
        ///     Gets or sets the play option name
        /// </summary>
        public string PlayOptionName { get; set; }

        /// <summary>
        ///     Gets or sets the minimum base RTP in percent.
        /// </summary>
        public decimal MinBaseRtpPercent { get; set; }

        /// <summary>
        ///     Gets or sets the maximum base RTP in percent.
        /// </summary>
        public decimal MaxBaseRtpPercent { get; set; }

        /// <summary>
        ///     Gets or sets the minimum SAP startup RTP in percent.
        /// </summary>
        public decimal MinSapStartupRtpPercent { get; set; }

        /// <summary>
        ///     Gets or sets the maximum SAP startup RTP in percent.
        /// </summary>
        public decimal MaxSapStartupRtpPercent { get; set; }

        /// <summary>
        ///     Gets or sets the SAP increment RTP in percent
        /// </summary>
        public decimal SapIncrementRtpPercent { get; set; }

        /// <summary>
        ///     Gets or sets the minimum Link progressive startup RTP in percent.
        /// </summary>
        public decimal MinLinkStartupRtpPercent { get; set; }

        /// <summary>
        ///     Gets or sets the maximum Link progressive startup RTP in percent.
        /// </summary>
        public decimal MaxLinkStartupRtpPercent { get; set; }

        /// <summary>
        ///     Gets or sets the Link progressive increment RTP in percent
        /// </summary>
        public decimal LinkIncrementRtpPercent { get; set; }
    }
}
