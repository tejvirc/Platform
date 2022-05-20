namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System;

    /// <summary>
    ///     Represents the status of a connection and the last time it was updated.
    /// </summary>
    public class ConnectionStatus : ICloneable
    {
        private ConnectionState _connectionState = ConnectionState.Disconnected;

        /// <summary>
        ///     Sets the connection state and updates the timestamp for the value
        /// </summary>
        public ConnectionState ConnectionState
        {
            get => _connectionState;
            set
            {
                _connectionState = value;
                StateTimestamp = DateTime.Now;
            }
        }

        /// <summary>
        ///     Timestamp for when the connection state was last updated
        /// </summary>
        public DateTime StateTimestamp { get; private set; } = DateTime.MinValue;

        /// <summary>
        ///     Since this is a reference type and we sometimes need to copy it
        /// </summary>
        public object Clone()
        {
            var newObject = new ConnectionStatus { ConnectionState = ConnectionState, StateTimestamp = StateTimestamp };
            return newObject;
        }
    }
}