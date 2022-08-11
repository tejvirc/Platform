namespace Aristocrat.Monaco.Protocol.Common.Communication
{
    using System;

    /// <summary>
    ///     Common interface for managing network connections which can receive data
    /// </summary>
    public interface IConnectionReader
    {
        /// <summary>
        ///     An observable that notifies of any incoming data to the connection from the server
        /// </summary>
        IObservable<Packet> IncomingBytes { get; }
    }
}