namespace Aristocrat.Bingo.Client.Configuration
{
    /// <summary>
    ///     The client configuration provider
    /// </summary>
    public interface IClientConfigurationProvider
    {
        /// <summary>
        ///     Gets the client configuration
        /// </summary>
        ClientConfigurationOptions Configuration { get; }
    }
}