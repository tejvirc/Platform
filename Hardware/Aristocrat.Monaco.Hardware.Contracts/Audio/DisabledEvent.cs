namespace Aristocrat.Monaco.Hardware.Contracts.Audio
{
    using Kernel;
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the Audio DisabledEvent class.</summary>
    /// <remarks>This event is posted when Audio device becomes Disabled.</remarks>
    public class DisabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent"/> class.
        /// </summary>
        /// <param name="reasons">Reasons for the disabled event.</param>
        public DisabledEvent(DisabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the disabled event.</summary>
        public DisabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"Audio {base.ToString()} {Reasons}");
        }
    }
}
