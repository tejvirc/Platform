namespace Aristocrat.Monaco.Bingo.Common.Storage.Model
{
    /// <summary>
    ///     The service for interfacing with the host data
    /// </summary>
    public interface IHostService
    {
        /// <summary>
        ///     Gets the bingo host from storage
        /// </summary>
        /// <returns>The bingo host found in storage</returns>
        Host GetHost();

        /// <summary>
        ///     Saves the bingo host to the persistence storage
        /// </summary>
        /// <params nme="host">The host to save</param>
        public void SaveHost(Host host);
    }
}