using static System.FormattableString;

namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    /// <inheritdoc />
    public class IdReaderTimeoutEvent : IdReaderBaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdReaderTimeoutEvent"/> class.
        /// </summary>
        public IdReaderTimeoutEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IdReaderTimeoutEvent"/> class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID..</param>
        public IdReaderTimeoutEvent(int idReaderId) : base(idReaderId)
        {
        }

        /// <inheritdoc />
        public override string ToString() => Invariant($"{base.ToString()} timeout");
    }
}
