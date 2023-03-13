namespace Aristocrat.Bingo.Client
{

    /// <summary>
    ///     States for the RPC Connection
    /// </summary>
    public enum RpcConnectionState
    {
        /// <summary>
        ///     The RPC client is trying to connect to the host
        /// </summary>
        Connecting,

        /// <summary>
        ///     The RPC client is connected and healthy
        /// </summary>
        Connected,

        /// <summary>
        ///     The RPC client is disconnected and needs to reconnect again
        /// </summary>
        Disconnected,

        /// <summary>
        ///     Thr RPC client is closed and no connects can be made
        /// </summary>
        Closed
    }
}