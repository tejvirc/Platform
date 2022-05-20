namespace Aristocrat.Monaco.Hardware.Contracts
{
    using Kernel;
    using SharedDevice;

    /// <summary>
    ///     Provides a mechanism to query the for configured hardware devices.
    /// </summary>
    public interface IDeviceRegistryService : IService
    {
        /// <summary>
        ///     Gets device interface
        /// </summary>
        /// <typeparam name="T">Device Interface</typeparam>
        /// <returns>Device or null</returns>
        T GetDevice<T>();

        /// <summary>
        ///     Adds device interface
        /// </summary>
        /// <typeparam name="T">Device Interface</typeparam>
        /// <returns>Device or null</returns>
        void AddDevice<T>(T device) where T : IDeviceService;

        /// <summary>
        ///     Removes the device interface
        /// </summary>
        /// <typeparam name="T">Device Interface</typeparam>
        void RemoveDevice<T>() where T : IDeviceService;

        /// <summary>
        ///     Removes the device interface
        /// </summary>
        /// <typeparam name="T">Device Interface</typeparam>
        void RemoveDevice<T>(T device) where T : IDeviceService;
    }
}