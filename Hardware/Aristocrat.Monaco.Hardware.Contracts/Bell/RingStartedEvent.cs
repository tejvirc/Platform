namespace Aristocrat.Monaco.Hardware.Contracts.Bell
{
    using Kernel;

    /// <summary>Definition of the RingStartedEvent class.</summary>
    public class RingStartedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return $"Bell {nameof(RingStartedEvent)}";
        }
    }
}