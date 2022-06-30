namespace Aristocrat.Monaco.Bingo
{
    using System;
    using System.Threading.Tasks;

    public interface IBingoClientConnectionState
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
        ///     Starts the bingo client connection handling
        /// </summary>
        /// <returns>A task for starting the bingo client connection</returns>
        Task Start();

        /// <summary>
        ///     Stops the bingo client connection handling
        /// </summary>
        /// <returns>A task for stopping the bingo client connection</returns>
        Task Stop();
    }
}