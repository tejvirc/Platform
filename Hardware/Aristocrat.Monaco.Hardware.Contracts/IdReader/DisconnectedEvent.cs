namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the ID Reader DisconnectedEvent class.</summary>
    /// <remarks>This event is posted when ID Reader is disconnected from the USB.</remarks>
    [Serializable]
    public class DisconnectedEvent : IdReaderBaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedEvent"/> class.
        /// </summary>
        public DisconnectedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisconnectedEvent"/> class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        public DisconnectedEvent(int idReaderId)
            : base(idReaderId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} disconnected");
        }
    }
}

