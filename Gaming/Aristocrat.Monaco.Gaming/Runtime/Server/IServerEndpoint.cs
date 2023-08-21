namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    /// <summary>
    ///     Provides a mechanism to
    /// </summary>
    public interface IServerEndpoint
    {
        /// <summary>
        ///     Start the server-side communications channel
        /// </summary>
        void Start();

        /// <summary>
        ///     Shutdown the server-side communications channel
        /// </summary>
        void Shutdown();
    }
}