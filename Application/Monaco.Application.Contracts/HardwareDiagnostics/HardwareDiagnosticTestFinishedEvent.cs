namespace Aristocrat.Monaco.Application.Contracts.HardwareDiagnostics
{
    using System;
    using Kernel;

    /// <summary>Definition of the Hardware Diagnostic Test Finish class.</summary>
    /// <remarks>This event is posted when any hardware device completes diagnostic tests.</remarks>
    [Serializable]
    public class HardwareDiagnosticTestFinishedEvent : BaseEvent
    {
        /// <summary>
        /// The category of device having diagnostic tests performed.
        /// </summary>
        public HardwareDiagnosticDeviceCategory DeviceCategory { get; private set; }

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
