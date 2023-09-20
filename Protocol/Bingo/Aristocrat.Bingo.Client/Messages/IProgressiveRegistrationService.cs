namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using Progressives;

    /// <summary>
    ///     The progressive registration service
    /// </summary>
    public interface IProgressiveRegistrationService
    {
        /// <summary>
        ///     Gets the progressive information from the server
        /// </summary>
        /// <param name="message">The progressive registration message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task for registering the client</returns>
        Task<ProgressiveRegistrationResults> RegisterClient(ProgressiveRegistrationMessage message, CancellationToken token = default);
    }
}
