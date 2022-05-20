namespace Aristocrat.Monaco.Hardware.Contracts.Bell
{
    using Kernel;

    /// <summary>Definition of the RingStoppedEvent class.</summary>
    public class RingStoppedEvent : BaseEvent
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return $"Bell {nameof(RingStoppedEvent)}";
        }
    }
}