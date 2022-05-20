namespace Aristocrat.Monaco.G2S
{
    using Data.Hosts;

    /// <summary>
    ///     Provides a mechanism to get access to services and repositories in the data layer
    /// </summary>
    public interface IG2SDataFactory
    {
        /// <summary>
        ///     Gets instance of <c>IHostService</c>.
        /// </summary>
        /// <returns>Returns instance of <c>IHostService</c>.</returns>
        IHostService GetHostService();
    }
}