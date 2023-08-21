namespace Aristocrat.Bingo.Client.Messages
{
    using System.Threading;
    using System.Threading.Tasks;
    using ServerApiGateway;

    /// <summary>
    ///     The configuration service
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        ///     Gets the client configuration from the server
        /// </summary>
        /// <param name="message">The configuration message</param>
        /// <param name="token">The cancellation token</param>
        /// <returns>Returns the task containing the client configuration settings from the server</returns>
        Task<ConfigurationResponse> ConfigureClient(ConfigurationMessage message, CancellationToken token);
    }
}
