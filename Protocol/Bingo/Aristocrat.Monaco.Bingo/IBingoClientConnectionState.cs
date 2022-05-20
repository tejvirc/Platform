namespace Aristocrat.Monaco.Bingo
{
    using System;

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
    }
}