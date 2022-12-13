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
        /// <returns>A whether or not the progressive update was handled</returns>
        Task<bool> ProcessProgressiveUpdate(ProgressiveUpdateMessage update, CancellationToken token);
    }
}
