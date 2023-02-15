namespace Aristocrat.Monaco.Application.Contracts.Detection
{
    using Hardware.Contracts.SharedDevice;
    using Kernel;

    /// <summary>
    ///     This event is raised during device detection for a detected device.
    /// </summary>
    public class DeviceDetectedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="device">device</param>
        public DeviceDetectedEvent(IDevice device)
        {
            Device = device;
        }

        /// <summary>
        ///     Get the device
        /// </summary>
        public IDevice Device { get; }
    }
}