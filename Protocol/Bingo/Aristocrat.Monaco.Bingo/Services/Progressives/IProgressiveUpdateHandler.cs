namespace Aristocrat.Monaco.Bingo.Services.Progressives
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages.Progressives;

    /// <summary>
    ///     Handles the progressive updates received
    /// </summary>
    public interface IProgressiveUpdateHandler
    {
        /// <summary>
        ///     Process the progressive updates from the server
        /// </summary>
        /// <param name="update">The progressive update message to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>Whether or not the progressive update was handled</returns>
        Task<bool> ProcessProgressiveUpdate(ProgressiveUpdateMessage update, CancellationToken token);

        /// <summary>
        ///     Process the disable by progressive message from the server
        /// </summary>
        /// <param name="disable">The disable by progressive message to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>Whether or not the disable by progressive was handled</returns>
        Task<bool> DisableByProgressive(DisableByProgressiveMessage disable, CancellationToken token);

        /// <summary>
        ///     Process the enable by progressive message from the server
        /// </summary>
        /// <param name="enable">The enable by progressive message to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>Whether or not the enable by progressive was handled</returns>
        Task<bool> EnableByProgressive(EnableByProgressiveMessage enable, CancellationToken token);
    }
}
