namespace Aristocrat.Monaco.PackageManifest.Models
{
    /// <summary>
    ///     Defines a return-to-player item
    /// </summary>
    public class ReturnToPlayerItem
    {
        /// <summary>
        ///     Gets or sets the play option identifier
        /// </summary>
        public int PlayOptionId { get; set; }

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
