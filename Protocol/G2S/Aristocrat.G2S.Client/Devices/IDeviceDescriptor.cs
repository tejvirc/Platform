namespace Aristocrat.G2S.Client.Devices
{
    /// <summary>
    ///     Provides a mechanism to interact with a devices descriptor info
    /// </summary>
    /// <typeparam name="TDevice">The device type</typeparam>
    public interface IDeviceDescriptor<in TDevice>
        where TDevice : IDevice
    {
        /// <summary>
        ///     Gets the devices descriptor info
        /// </summary>
        /// <param name="device">The device</param>
        /// <returns>The descriptor info</returns>
        Descriptor GetDescriptor(TDevice device);
    }
}