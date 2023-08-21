namespace Aristocrat.Monaco.Hardware.Contracts.Bell
{
    using Kernel;
    using SharedDevice;

    /// <summary>Definition of the EnabledEvent class.</summary>
    public class EnabledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(EnabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the enabled event.</summary>
        public EnabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Bell {nameof(EnabledEvent)} {Reasons}";
        }
    }
}