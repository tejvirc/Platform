namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using Kernel;
    using ProtoBuf;
    using static System.FormattableString;

    /// <summary>Definition of the IdReaderBaseEvent class.</summary>
    /// <remarks>All other idReader events are derived from this event.</remarks>
    [ProtoContract]
    public class IdReaderBaseEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="IdReaderBaseEvent"/> class.
        /// </summary>
        protected IdReaderBaseEvent()
        {
            IdReaderId = 1;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="IdReaderBaseEvent"/> class.
        /// </summary>
        /// <param name="idReaderId">The ID of the idReader associated with the event.</param>
        protected IdReaderBaseEvent(int idReaderId)
        {
            IdReaderId = idReaderId;
        }

        /// <summary>
        ///     Gets the ID of the idReader associated with the event.
        /// </summary>
        [ProtoMember(1)]
        public int IdReaderId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{GetType().Name} [IdReaderId={IdReaderId}]");
        }
    }
}
