namespace Aristocrat.G2S.Client.Devices
{
    using System.Collections.Generic;

    /// <summary>
    ///     Defines a contract for a device connector
    /// </summary>
    public interface IDeviceConnector
    {
        /// <summary>
        ///     Gets a list of registered devices
        /// </summary>
        IEnumerable<IDevice> Devices { get; }

        /// <summary>
        ///     Gets a device by it's key
        /// </summary>
        /// <param name="deviceClass">The device class</param>
        /// <param name="deviceId">The device identifier</param>
        /// <returns>The device if found or null</returns>
        IDevice GetDevice(string deviceClass, int deviceId);

        /// <summary>
        ///     Gets a specific device. Throws if there is more than one device of the requested type
        /// </summary>
        /// <typeparam name="TDevice">The device type</typeparam>
        /// <returns>The device if found or null</returns>
        TDevice GetDevice<TDevice>()
            where TDevice : IDevice;

        /// <summary>
        ///     Gets a list of specific devices
        /// </summary>
        /// <typeparam name="TDevice">The device type</typeparam>
        /// <returns>The device if found or null</returns>
        IEnumerable<TDevice> GetDevices<TDevice>()
            where TDevice : IDevice;

        /// <summary>
        ///     Gets a specific device for the specified device identifier
        /// </summary>
        /// <typeparam name="TDevice">The device type</typeparam>
        /// <param name="deviceId">The device identifier</param>
        /// <returns>The device if found or null</returns>
        TDevice GetDevice<TDevice>(int deviceId)
            where TDevice : IDevice;

        /// <summary>
        ///     Adds a device
        /// </summary>
        /// <param name="device">The device</param>
        /// <returns>The added device</returns>
        IDevice AddDevice(IDevice device);

        /// <summary>
        ///     Removes a device
        /// </summary>
        /// <param name="device">The device</param>
        /// <returns>The removed device</returns>
        IDevice RemoveDevice(IDevice device);
    }
}