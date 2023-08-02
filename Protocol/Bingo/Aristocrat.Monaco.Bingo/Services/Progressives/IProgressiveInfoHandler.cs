namespace Aristocrat.Monaco.Bingo.Services.Progressives
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages.Progressives;

    /// <summary>
    ///     Handles the progressive information received
    /// </summary>
    public interface IProgressiveInfoHandler
    {
        /// <summary>
        ///     Process the progressive information from the server
        /// </summary>
        /// <param name="info">The progressive information message to process</param>
        /// <param name="token">The cancellation token for this task</param>
        /// <returns>Whether or not the progressive information was handled</returns>
        Task<bool> ProcessProgressiveInfo(ProgressiveInfoMessage info, CancellationToken token);
    }
}
