namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    public interface IServerConfigurationProvider
    {
        /// <summary>
        ///     Gets the server settings from the database.
        /// </summary>
        /// <returns>A model which holds the acquired server settings </returns>
        BingoServerSettingsModel GetServerConfiguration();
    }
}