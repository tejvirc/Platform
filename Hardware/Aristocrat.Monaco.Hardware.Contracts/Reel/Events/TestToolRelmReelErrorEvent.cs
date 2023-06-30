namespace Aristocrat.Monaco.Hardware.Contracts.Reel.Events
{
    using System.Text;
    using Kernel;

    /// <summary>
    ///     Publish this event to inject a Relm reel error interrupt into the Relm controller.
    /// </summary>
    public class TestToolRelmReelErrorEvent : BaseEvent
    {
        /// <summary>
        ///     The reel status.
        /// </summary>
        public ReelStatus ReelStatus { get; set; }

        /// <summary>
        ///     The light status
        /// </summary>
        public LightStatus LightStatus { get; set; }

        /// <summary>
        ///     Is the event queue full?
        /// </summary>
        public bool IsEventQueueFull { get; set; }

        /// <summary>
        ///     Did the ping timeout?
        /// </summary>
        public bool PingTimeout { get; set; }

        /// <summary>
        ///     Clear the ping timeout?
        /// </summary>
        public bool ClearPingTimeout { get; set; }
    }
}
