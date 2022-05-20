namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using static System.FormattableString;

    /// <summary>Definition of the ID Reader ConnectedEvent class.</summary>
    /// <remarks>This event is posted when ID Reader is connected to the USB.</remarks>
    [Serializable]
    public class ConnectedEvent : IdReaderBaseEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedEvent"/> class.
        /// </summary>
        public ConnectedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectedEvent"/> class with the ID reader's ID.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID..</param>
        public ConnectedEvent(int idReaderId)
            : base(idReaderId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} connected");
        }
    }
}
