namespace Aristocrat.Monaco.Hardware.Contracts
{
    using Kernel;
    using SharedDevice;

    /// <summary>
    ///     The Firmware installed  event is posted when the firmware installer has completed
    /// </summary>
    public class FirmwareInstalledEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FirmwareInstalledEvent" /> class.
        /// </summary>
        /// <param name="packageId">The installed package id</param>
        /// <param name="device">Device type.</param>
        public FirmwareInstalledEvent(string packageId, DeviceType device)
        {
            PackageId = packageId;
            Device = device;
        }

        /// <summary>
        ///     Gets the installed package id
        /// </summary>
        public string PackageId { get; }

        /// <summary>Gets the device type.</summary>
        public DeviceType Device { get; }
    }
}