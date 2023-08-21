namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    public interface IBingoDataFactory
    {
        /// <summary>
        ///     Gets the configuration provider to access the saved bingo server settings.
        /// </summary>
        /// <returns>The bingo server configuration provider</returns>
        IServerConfigurationProvider GetConfigurationProvider();

        /// <summary>
        ///     Gets the host service
        /// </summary>
        /// <returns>The host service</returns>
        IHostService GetHostService();
    }
}
