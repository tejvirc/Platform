namespace Aristocrat.Monaco.Hardware.Contracts.HardMeter
{
    /// <summary>Valid hard meter state enumerations.</summary>
    public enum HardMeterState
    {
        /// <summary>Indicates hard meter state uninitialized.</summary>
        Uninitialized = 0,

        /// <summary>Indicates hard meter state disabled.</summary>
        Disabled,

        /// <summary>Indicates hard meter state enabled.</summary>
        Enabled,

        /// <summary>Indicates hard meter state error.</summary>
        Error
    }

    /// <summary>Valid hard meter action enumerations.</summary>
    public enum HardMeterAction
    {
        /// <summary>Indicates hard meter action on.</summary>
        On = 0,

        /// <summary>Indicates hard meter action off.</summary>
        Off
    }

    /// <summary>Class to be used in a generic List for management of logical hard meters.</summary>
    public class LogicalHardMeter : LogicalDeviceBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalHardMeter" /> class.
        /// </summary>
        public LogicalHardMeter()
        {
            LogicalId = 0;
            State = HardMeterState.Uninitialized;
            Action = HardMeterAction.Off;
            TickValue = 1;
            Ready = false;
            Suspended = false;
            Count = 0;
            IsAvailable = true;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogicalHardMeter" /> class.
        /// </summary>
        /// <param name="physicalId">The physical hard meter ID.</param>
        /// <param name="logicalId">The logical id</param>
        /// <param name="name">The hard meter name.</param>
        /// <param name="localizedName">The localized hard meter name.</param>
        /// <param name="tickValue">The hard meter tick value.</param>
        /// <param name="isAvailable">Is hard meter available</param>
        public LogicalHardMeter(
            int physicalId,
            int logicalId,
            string name,
            string localizedName,
            long tickValue,
            bool isAvailable)
            : base(physicalId, name, localizedName)
        {
            LogicalId = logicalId;
            State = HardMeterState.Uninitialized;
            Action = HardMeterAction.Off;
            TickValue = tickValue;
            Ready = true;
            Suspended = false;
            Count = 0;
            IsAvailable = isAvailable;
        }
        /// <summary> Gets or sets Logical Meter Id </summary>
        public int LogicalId { get; set; }

        /// <summary>Gets or sets a value for hard meter state.</summary>
        public HardMeterState State { get; set; }

        /// <summary>Gets or sets a value for hard meter action.</summary>
        public HardMeterAction Action { get; set; }

        /// <summary>Gets or sets the value for hard meter tick.</summary>
        public long TickValue { get; set; }

        /// <summary>Gets or sets a value indicating whether a hard meter tick has completed.</summary>
        public bool Ready { get; set; }

        /// <summary>Gets or sets a value indicating whether a hard meter has been suspended.</summary>
        public bool Suspended { get; set; }

        /// <summary>Gets or sets the value for the hard meter count.</summary>
        public long Count { get; set; }

        /// <summary>Is hard meter available</summary>
        public bool IsAvailable { get; set; }
    }
}