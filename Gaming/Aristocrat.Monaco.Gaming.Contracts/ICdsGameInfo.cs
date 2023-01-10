namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Game Information for CDS games
    /// </summary>
    public interface ICdsGameInfo
    {
        /// <summary>
        ///     The identifier for this cds game information
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     The minimum wager credits for this item
        /// </summary>
        int MinWagerCredits { get; }

        /// <summary>
        ///     The maximum wager credits for this item
        /// </summary>
        int MaxWagerCredits { get; }
    }
}