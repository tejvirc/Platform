namespace Aristocrat.Monaco.Application.Contracts.Detection
{
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
        public DeviceDetectedEvent(SupportedDevicesDevice device)
        {
            Device = device;
        }

        /// <summary>
        ///     Get the device
        /// </summary>
        public SupportedDevicesDevice Device { get; }

        /// <summary>
        ///     ToString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name} {Device.Type} {Device.Name}";
        }
    }
}