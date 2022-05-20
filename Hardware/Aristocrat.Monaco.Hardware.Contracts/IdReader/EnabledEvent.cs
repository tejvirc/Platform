namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using System;
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the ID Reader EnabledEvent class.</summary>
    /// <remarks>This event is posted when ID Reader becomes Enabled.</remarks>
    [Serializable]
    public class EnabledEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent"/> class.
        /// </summary>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(EnabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent"/> class.Initializes a new instance of the EnabledEvent
        ///     class with the ID reader's ID and enabled reasons.
        /// </summary>
        /// <param name="idReaderId">The associated ID reader's ID.</param>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(int idReaderId, EnabledReasons reasons)
            : base(idReaderId)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the enabled event.</summary>
        public EnabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"IdReader {base.ToString()} [{Reasons}]");
        }
    }
}
