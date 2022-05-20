namespace Aristocrat.Monaco.Hardware.Contracts.Bell
{
    using Kernel;
    using SharedDevice;

    /// <summary>Definition of the DisabledEvent class.</summary>
    public class DisabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DisabledEvent" /> class.
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
            return $"Bell {nameof(DisabledEvent)} {Reasons}";
        }
    }
}