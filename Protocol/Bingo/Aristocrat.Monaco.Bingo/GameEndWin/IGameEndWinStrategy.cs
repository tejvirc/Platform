namespace Aristocrat.Monaco.Bingo.GameEndWin
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     A strategy for handled GEW prizes
    /// </summary>
    public interface IGameEndWinStrategy
    {
        /// <summary>
        ///     Process the game end win for the strategy
        /// </summary>
        /// <param name="winAmount">The amount provided for this win</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>Whether or not the game end win was awarded</returns>
        Task<bool> ProcessWin(long winAmount, CancellationToken token = default);

        /// <summary>
        ///     Recovers the game end win for the provide game transaction
        /// </summary>
        /// <param name="gameTransactionId">The game transaction to recover</param>
        /// <param name="token">A cancellation token</param>
        /// <returns>Whether or not the game end win was awarded</returns>
        Task<bool> Recover(long gameTransactionId, CancellationToken token);
    }
}