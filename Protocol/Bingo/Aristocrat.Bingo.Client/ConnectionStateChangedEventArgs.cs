namespace Aristocrat.Bingo.Client
{
    using System;

    public sealed class ConnectionStateChangedEventArgs : EventArgs
    {
        public RpcConnectionState State { get; }

        public ConnectionStateChangedEventArgs(RpcConnectionState state)
        {
            State = state;
        }
    }
}