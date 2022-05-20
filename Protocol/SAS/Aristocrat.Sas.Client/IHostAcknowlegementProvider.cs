namespace Aristocrat.Sas.Client
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     The Handler for SAS implied ACKs
    /// </summary>
    public interface IHostAcknowlegementProvider : IDisposable, IHostAcknowledgementHandler
    {
        /// <summary>
        ///     The event handler used when synchronization is lost
        /// </summary>
        event EventHandler SynchronizationLost;

        /// <summary>
        ///     Gets whether or not the system is synchronized
        /// </summary>
        bool Synchronized { get; }

        /// <summary>
        ///     Gets whether or not the last message has been Nack'd
        /// </summary>
        bool LastMessageNacked { get; }

        /// <summary>
        ///     Called when a link down occurs
        /// </summary>
        void LinkDown();

        /// <summary>
        ///     Check whether or not the messages have had a successful implied ack
        /// </summary>
        /// <param name="globalBroadcast">Whether or not the current message is a global broadcast message</param>
        /// <param name="otherAddressPoll">Whether or not the current message is for another SAS address</param>
        /// <param name="data">The data for the current received message</param>
        /// <returns>Whether or not the messages have had a successful implied ack</returns>
        bool CheckImpliedAck(bool globalBroadcast, bool otherAddressPoll, IReadOnlyCollection<byte> data);

        /// <summary>
        ///     Used to set the data to validate using an implied ack
        /// </summary>
        /// <param name="data">The data to wait for an implied on with</param>
        /// <param name="handlers">The handlers to use when ACKing or NACKing a message</param>
        void SetPendingImpliedAck(IReadOnlyCollection<byte> data, IHostAcknowledgementHandler handlers);
    }
}