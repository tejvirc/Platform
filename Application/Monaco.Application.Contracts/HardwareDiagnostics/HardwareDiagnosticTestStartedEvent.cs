namespace Aristocrat.Monaco.Application.Contracts.HardwareDiagnostics
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>Definition of the Hardware Diagnostic Test Start class.</summary>
    /// <remarks>This event is posted when any hardware device starts diagnostic tests.</remarks>
    [ProtoContract]
    public class HardwareDiagnosticTestStartedEvent : BaseEvent
    {
        /// <summary>
        /// The category of device having diagnostic tests performed.
        /// </summary>
        [ProtoMember(1)]
        public HardwareDiagnosticDeviceCategory DeviceCategory { get; private set; }

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public HardwareDiagnosticTestStartedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HardwareDiagnosticTestStartedEvent"/> class with the type of hardware being tested.
        /// </summary>
        /// <param name="deviceCategory">The category of device having diagnostic tests performed.</param>
        public HardwareDiagnosticTestStartedEvent(HardwareDiagnosticDeviceCategory deviceCategory)
        {
            DeviceCategory = deviceCategory;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Hardware diagnostic test for {DeviceCategory} started";
        }
    }
}
