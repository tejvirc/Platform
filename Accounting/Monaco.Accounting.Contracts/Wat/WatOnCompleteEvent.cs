namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event emitted when a Wat on transfer has completed.
    /// </summary>
    /// <remarks>
    ///     This event is posted  when the transfer on request from the transfer host is
    ///     considered complete, whether successful or not. The client can perform any clean up code here
    ///     to prepare for normal operations.
    /// </remarks>
    [ProtoContract]
    public class WatOnCompleteEvent : BaseEvent
    {

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public WatOnCompleteEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WatOnCompleteEvent" /> class.
        /// </summary>
        /// <param name="transaction">The completed WAT transaction</param>
        public WatOnCompleteEvent(WatOnTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        ///     Gets the completed Wat Transaction
        /// </summary>
        [ProtoMember(1)]
        public WatOnTransaction Transaction { get; }
    }
}