namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Newtonsoft.Json;

    /// <summary>
    ///     Some paytables vary the theoretical payback percentage of the game based upon wager.These variants are identified
    ///     as wager categories.
    /// </summary>
    public class WagerCategory : IWagerCategory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="WagerCategory" /> class.
        /// </summary>
        /// <param name="id">Wager category identifier</param>
        /// <param name="theoPaybackPercent">Theoretical payback percentage associated with the wager category</param>
        public WagerCategory(string id, decimal theoPaybackPercent)
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
        [JsonConstructor]
        public WagerCategory(
            string id,
            decimal theoPaybackPercent,
            int? minWagerCredits,
            int? maxWagerCredits,
            long maxWinAmount)
            : this(id, theoPaybackPercent)
        {
            MinWagerCredits = minWagerCredits;
            MaxWagerCredits = maxWagerCredits;
            MaxWinAmount = maxWinAmount;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public decimal TheoPaybackPercent { get; }

        /// <inheritdoc />
        public int? MinWagerCredits { get; }

        /// <inheritdoc />
        public int? MaxWagerCredits { get; }

        /// <inheritdoc />
        public long MaxWinAmount { get; }
    }
}