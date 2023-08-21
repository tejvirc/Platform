namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Provides a mechanism to interact with a Free Game
    /// </summary>
    public interface IFreeGameHistory
    {
        /// <summary>
        ///     Used to start the free game
        /// </summary>
        void StartFreeGame();

        /// <summary>
        ///     Used to post the final results.
        /// </summary>
        /// <param name="finalWin">The total amount won.</param>
        void FreeGameResults(long finalWin);

        /// <summary>
        ///     Used to end the free game
        /// </summary>
        /// <returns>The updated free game</returns>
        IFreeGameInfo EndFreeGame();
    }
}
