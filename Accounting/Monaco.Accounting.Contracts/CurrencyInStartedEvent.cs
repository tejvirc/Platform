namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Hardware.Contracts.NoteAcceptor;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event emitted when a currency has been inserted into the note acceptor and the request
    ///     has been identified eligible for processing.
    /// </summary>
    /// <remarks>
    ///     This event only signals the start of handling a currency-in request. It is posted before
    ///     the note acceptor starts stacking the currency.
    /// </remarks>
    [ProtoContract]
    public class CurrencyInStartedEvent : BaseEvent
    {
        /// <summary>
        /// Empty constructor for Deserialization
        /// </summary>
        public CurrencyInStartedEvent()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="note"></param>
        public CurrencyInStartedEvent(INote note)
        {
            Note = note;
        }

        /// <summary>Note</summary>
        [ProtoMember(1)]
        public INote Note { get; }
    }
}