namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the DisabledEvent class.</summary>
    public class DisabledEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(DisabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent"/> class.
        /// </summary>
        /// <param name="reelControllerId">The ID of the note acceptor associated with the event.</param>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(int reelControllerId, DisabledReasons reasons)
            : base(reelControllerId)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the disabled event.</summary>
        public DisabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Reasons={Reasons}]");
        }
    }
}