namespace Aristocrat.Monaco.Application.Contracts.HardwareDiagnostics
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>Definition of the Hardware Diagnostic Test Finish class.</summary>
    /// <remarks>This event is posted when any hardware device completes diagnostic tests.</remarks>
    [ProtoContract]
    public class HardwareDiagnosticTestFinishedEvent : BaseEvent
    {
        /// <summary>
        /// The category of device having diagnostic tests performed.
        /// </summary>
        [ProtoMember(1)]
        public HardwareDiagnosticDeviceCategory DeviceCategory { get; private set; }

        /// <summary>
        /// Empty constructor for deserialization
        /// </summary>
        public HardwareDiagnosticTestFinishedEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HardwareDiagnosticTestFinishedEvent"/> class with the type of hardware being tested.
        /// </summary>
        /// <param name="deviceCategory">The category of device having diagnostic tests performed.</param>
        public HardwareDiagnosticTestFinishedEvent(HardwareDiagnosticDeviceCategory deviceCategory)
        {
            DeviceCategory = deviceCategory;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Hardware diagnostic test for ${DeviceCategory} finished";
        }
    }
}
