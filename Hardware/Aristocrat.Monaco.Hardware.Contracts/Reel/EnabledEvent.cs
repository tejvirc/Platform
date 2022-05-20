namespace Aristocrat.Monaco.Hardware.Contracts.Reel
{
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the EnabledEvent class.</summary>
    public class EnabledEvent : ReelControllerBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(EnabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.
        /// </summary>
        /// <param name="reelControllerId">The associated reel controller ID</param>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(int reelControllerId, EnabledReasons reasons)
            : base(reelControllerId)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the enabled event.</summary>
        public EnabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [Reasons={Reasons}]");
        }
    }
}