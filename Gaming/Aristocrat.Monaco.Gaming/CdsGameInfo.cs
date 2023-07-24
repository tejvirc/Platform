namespace Aristocrat.Monaco.Gaming
{
    using Contracts;

    /// <summary>
    ///     The <see cref="CdsGameInfo" /> contains game information for central determinant games.
    /// </summary>
    public class CdsGameInfo : ICdsGameInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CdsGameInfo" /> class
        /// </summary>
        /// <param name="id">The identifier for this cds game information</param>
        /// <param name="minWagerCredits">The minimum wager credits for this item</param>
        /// <param name="maxWagerCredits">The maximum wager credits for this item</param>
        public CdsGameInfo(string id, int minWagerCredits, int maxWagerCredits)
        {
            Id = id;
            MinWagerCredits = minWagerCredits;
            MaxWagerCredits = maxWagerCredits;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public int MinWagerCredits { get; }

        /// <inheritdoc />
        public int MaxWagerCredits { get; }
    }
}