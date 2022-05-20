namespace Aristocrat.G2S.Client.Configuration
{
    using Devices;

    /// <summary>
    ///     Details about one device for which a host is registered as owner.
    /// </summary>
    public class OwnedDevice
    {
        /// <summary>
        ///     Gets or sets the owned device.
        /// </summary>
        public IDevice Device { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether the device is active (true) or inactive.
        /// </summary>
        public bool Active { get; set; }
    }
}