namespace Aristocrat.Monaco.Hhr.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Messages;

    /// <summary>
    ///     Service responsible to attempt to recover when previous game play request fails to fetch response from server.
    /// </summary>
    public interface IGameRecoveryService
    {
        /// <summary>
        ///     Attempts recovery, which includes checking whether we have request waiting for a response.
        ///     Sends Recovery request to server and adapts recovery response to game play response.
        /// </summary>
        /// <returns>Returns GamePlayResponse which is converted from GameRecoveryResponse.</returns>
        Task<GamePlayResponse> Recover(uint requestSequenceId, CancellationToken token = default);
    }
}