namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism for push-based notifications when a device changes.
    /// </summary>
    public interface IDeviceObserver
    {
        /// <summary>
        ///     Notifies the observer that the device has changed.
        /// </summary>
        /// <param name="device">The device.</param>
        /// <param name="propertyName">The property name.</param>
        void Notify(IDevice device, string propertyName);
    }
}