namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Newtonsoft.Json;


    /// <summary>
    /// A wager category is a subset of a paytable with a specific payback percentage. A paytable may be made up of one or
    ///     more wager categories. Wager categories may be based on the amount wagered and/or the type of wager. Some paytables vary the theoretical payback percentage of the game based upon wager. These variants are identified
    ///     as wager categories.
    /// </summary>
    /// <seealso cref="IWagerCategory" />
    public class WagerCategory : IWagerCategory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WagerCategory" /> class.
        /// </summary>
        /// <param name="id">Wager category identifier</param>
        /// <param name="theoPaybackPercent">Theoretical payback percentage associated with the wager category</param>
        public WagerCategory(
            string id,
            decimal theoPaybackPercent)
        {
            Id = id;
            TheoPaybackPercent = theoPaybackPercent;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WagerCategory" /> class.
        /// </summary>
        /// <param name="id">Wager category identifier</param>
        /// <param name="theoPaybackPercent">Theoretical payback percentage associated with the wager category</param>
        /// <param name="minWagerCredits">Minimum wager, in credits, associated with the wager category</param>
        /// <param name="maxWagerCredits">Maximum wager, in credits, associated with the wager category</param>
        /// <param name="maxWinAmount">Maximum win amount, in millicents, associated with the wager category</param>
        public WagerCategory(
            string id,
            decimal theoPaybackPercent,
            int minWagerCredits,
            int maxWagerCredits,
            long maxWinAmount)
            : this(id, theoPaybackPercent)
        {
            MinWagerCredits = minWagerCredits;
            MaxWagerCredits = maxWagerCredits;
            MaxWinAmount = maxWinAmount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WagerCategory"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="theoPaybackPercent">The theoretical payback percentage associated with the wager category.</param>
        /// <param name="minWagerCredits">The minimum wager, in credits.</param>
        /// <param name="maxWagerCredits">The maximum wager, in credits.</param>
        /// <param name="maxWinAmount">The maximum win amount, in millicents.</param>
        /// <param name="minBaseRtpPercent">The minimum base RTP percent.</param>
        /// <param name="maxBaseRtpPercent">The maximum base RTP percent.</param>
        /// <param name="minSapStartupRtpPercent">The minimum sap startup RTP percent.</param>
        /// <param name="maxSapStartupRtpPercent">The maximum sap startup RTP percent.</param>
        /// <param name="sapIncrementRtpPercent">The sap increment RTP percent.</param>
        /// <param name="minLinkStartupRtpPercent">The minimum link startup RTP percent.</param>
        /// <param name="maxLinkStartupRtpPercent">The maximum link startup RTP percent.</param>
        /// <param name="linkIncrementRtpPercent">The link increment RTP percent.</param>
        [JsonConstructor]
        public WagerCategory(
            string id,
            decimal theoPaybackPercent,
            int minWagerCredits,
            int maxWagerCredits,
            long maxWinAmount,
            decimal minBaseRtpPercent,
            decimal maxBaseRtpPercent,
            decimal minSapStartupRtpPercent,
            decimal maxSapStartupRtpPercent,
            decimal sapIncrementRtpPercent,
            decimal minLinkStartupRtpPercent,
            decimal maxLinkStartupRtpPercent,
            decimal linkIncrementRtpPercent)
            : this(id, theoPaybackPercent, minWagerCredits, maxWagerCredits, maxWinAmount)
        {
            Id = id;
            TheoPaybackPercent = theoPaybackPercent;
            MinWagerCredits = minWagerCredits;
            MaxWagerCredits = maxWagerCredits;
            MaxWinAmount = maxWinAmount;
            MinBaseRtpPercent = minBaseRtpPercent;
            MaxBaseRtpPercent = maxBaseRtpPercent;
            MinSapStartupRtpPercent = minSapStartupRtpPercent;
            MaxSapStartupRtpPercent = maxSapStartupRtpPercent;
            SapIncrementRtpPercent = sapIncrementRtpPercent;
            MinLinkStartupRtpPercent = minLinkStartupRtpPercent;
            MaxLinkStartupRtpPercent = maxLinkStartupRtpPercent;
            LinkIncrementRtpPercent = linkIncrementRtpPercent;
        }

        /// <inheritdoc />
        public string Id { get; set; }

        /// <inheritdoc />
        public decimal TheoPaybackPercent { get; set; }

        /// <inheritdoc />
        public int? MinWagerCredits { get; set; }

        /// <inheritdoc />
        public int? MaxWagerCredits { get; set; }

        /// <inheritdoc />
        public long MaxWinAmount { get; set; }

        /// <inheritdoc />
        public decimal MinBaseRtpPercent { get; set; }

        /// <inheritdoc />
        public decimal MaxBaseRtpPercent { get; set; }

        /// <inheritdoc />
        public decimal MinSapStartupRtpPercent { get; set; }

        /// <inheritdoc />
        public decimal MaxSapStartupRtpPercent { get; set; }

        /// <inheritdoc />
        public decimal SapIncrementRtpPercent { get; set; }

        /// <inheritdoc />
        public decimal MinLinkStartupRtpPercent { get; set; }

        /// <inheritdoc />
        public decimal MaxLinkStartupRtpPercent { get; set; }

        /// <inheritdoc />
        public decimal LinkIncrementRtpPercent { get; set; }

        /// <summary>
        ///     Converts to a string.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(TheoPaybackPercent)}: {TheoPaybackPercent}";
        }
    }
}