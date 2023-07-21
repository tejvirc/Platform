namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Threading.Tasks;

    public interface IBaseClientConnectionState
    {
        /// <summary>
        ///     An event handler for when the client is fully connected and configured with bingo server
        /// </summary>
        event EventHandler ClientConnected;

        /// <summary>
        ///     An event handler for when the client is disconnected from the bingo server
        /// </summary>
        event EventHandler ClientDisconnected;

        /// <summary>
        ///     Starts the client connection handling
        /// </summary>
        /// <returns>A task for starting the client connection</returns>
        Task Start();

        /// <summary>
        ///     Stops the client connection handling
        /// </summary>
        /// <returns>A task for stopping the client connection</returns>
        Task Stop();
    }
}