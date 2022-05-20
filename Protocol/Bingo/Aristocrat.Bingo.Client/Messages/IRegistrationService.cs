namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     The registration service
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>
        ///     Registers the client
        /// </summary>
        /// <param name="message">The registration message</param>
        /// <param name="token">The cancellation token to use</param>
        /// <returns>Returns the task for registering the client</returns>
        Task<RegistrationResults> RegisterClient(RegistrationMessage message, CancellationToken token = default);
    }
}