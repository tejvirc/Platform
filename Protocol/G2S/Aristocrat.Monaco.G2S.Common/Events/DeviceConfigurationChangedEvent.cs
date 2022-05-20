namespace Aristocrat.Monaco.G2S.Common.Events
{
    using Aristocrat.G2S.Client.Devices;
    using Kernel;

    /// <summary>
    ///     The DeviceConfigurationChangedEvent is posted whenever an active device has changed.
    /// </summary>
    public class DeviceConfigurationChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DeviceConfigurationChangedEvent" /> class.
        /// </summary>
        /// <param name="device">The device.</param>
        public DeviceConfigurationChangedEvent(IDevice device)
        {
            Device = device;
        }

        /// <summary>
        ///     Gets the Device
        /// </summary>
        public IDevice Device { get; }
    }
}