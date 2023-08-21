namespace Aristocrat.Monaco.Hardware.Contracts.HardMeter
{
    /// <summary>
    /// The event fired when hard meter completes the ticks when a soft meter value changes
    /// </summary>
    public class HardMeterTickStoppedEvent : HardMeterBaseEvent
    {
        /// <summary>
        /// Event fired when hard meter stops ticking 
        /// </summary>
        /// <param name="logicalId">The logical id of hard meter</param>
        public HardMeterTickStoppedEvent(int logicalId) : base(logicalId)
        {
        }
    }
}
