namespace Aristocrat.Monaco.Application.Contracts.Detection
{
    using Hardware.Contracts.SharedDevice;
    using Kernel;

    /// <summary>
    ///     This event is raised during device detection upon completion of
    ///     all options without finding a device.
    /// </summary>
    public class DeviceNotDetectedEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="deviceType">device type</param>
        public DeviceNotDetectedEvent(DeviceType deviceType)
        {
            DeviceType = deviceType;
        }

        /// <summary>
        ///     Get the device type
        /// </summary>
        public DeviceType DeviceType { get; }

        /// <summary>
        ///     ToString override
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{GetType().Name} {DeviceType}";
        }
    }
}