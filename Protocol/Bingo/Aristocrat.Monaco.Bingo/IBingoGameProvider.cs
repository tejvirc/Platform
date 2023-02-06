namespace Aristocrat.Monaco.Bingo
{
    using Common.Storage;

    /// <summary>
    ///     The provider for the current bingo game information
    /// </summary>
    public interface IBingoGameProvider
    {
        /// <summary>
        ///     Gets the bingo game description for the current active game
        /// </summary>
        /// <returns>The active bingo game description</returns>
        BingoGameDescription GetBingoGame();
    }
}