namespace Aristocrat.Monaco.Hardware.Contracts.Communicator
{
    /// <summary>A dfu device status.</summary>
    public class DfuDeviceStatus
    {
        /// <summary>Gets or sets the state.</summary>
        /// <value>The state.</value>
        public DfuState State { get; set; }

        /// <summary>Gets or sets the timeout.</summary>
        /// <value>The timeout.</value>
        public int Timeout { get; set; }

        /// <summary>Gets or sets the status.</summary>
        /// <value>The status.</value>
        public DfuStatus Status { get; set; }
    }
}