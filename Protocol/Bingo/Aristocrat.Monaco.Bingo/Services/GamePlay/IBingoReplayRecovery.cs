namespace Aristocrat.Monaco.Bingo.Services.GamePlay
{
    using System.Threading;
    using System.Threading.Tasks;
    using Gaming.Contracts;

    /// <summary>
    ///     An interface for handling replay and recovery for bingo specifics such as bingo cards and ball call
    /// </summary>
    public interface IBingoReplayRecovery
    {
        /// <summary>
        ///     Recovers the last game's bingo displays
        /// </summary>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>The recovery task</returns>
        public Task RecoverDisplay(CancellationToken token);

        /// <summary>
        ///     Recovers the last game's bingo game
        /// </summary>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>The recovery task</returns>
        public Task RecoverGamePlay(CancellationToken token);

        /// <summary>
        ///     Replays the request game's bingo display
        /// </summary>
        /// <param name="log">The game to replay</param>
        /// <param name="finalizeReplay">Whether or not the replay needs to be finalized for show GEW and the entire ball call</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>The recovery task</returns>
        public Task Replay(IGameHistoryLog log, bool finalizeReplay, CancellationToken token);
    }
}