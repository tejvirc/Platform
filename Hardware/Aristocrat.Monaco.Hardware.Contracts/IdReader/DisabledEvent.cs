namespace Aristocrat.Monaco.Hardware.Contracts.IdReader
{
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the ID Reader DisabledEvent class.</summary>
    /// <remarks>This event is posted when ID Reader becomes disabled.</remarks>
    public class DisabledEvent : IdReaderBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent"/> class.
        /// </summary>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(DisabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent"/> class.
        /// </summary>
        /// <param name="idReaderId">The associated ID Reader's ID.</param>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(int idReaderId, DisabledReasons reasons)
            : base(idReaderId)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the disabled event.</summary>
        public DisabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"Id Reader {base.ToString()} {Reasons}");
        }
    }
}
