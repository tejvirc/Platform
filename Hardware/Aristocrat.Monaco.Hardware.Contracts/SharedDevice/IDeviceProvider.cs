namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    using System.Collections.Generic;
    using Communicator;

    /// <summary>
    ///     Interface for services supporting multiple devices
    /// </summary>
    public interface IDeviceProvider<out TAdapter>
        where TAdapter : IDeviceAdapter
    {
        /// <summary>Indexer to get items within this collection using array index syntax.</summary>
        /// <param name="key">The key.</param>
        /// <returns>The indexed item.</returns>
        TAdapter this[int key] { get; }

        /// <summary>Gets a value indicating whether the provider is initialized.</summary>
        /// <value>True if initialized, false if not.</value>
        bool Initialized { get; }

        /// <summary>Gets the adapters.</summary>
        /// <value>The adapters.</value>
        IEnumerable<TAdapter> Adapters { get; }

        /// <summary>
        ///     Remove the adapters
        /// </summary>
        void ClearAdapters();

        /// <summary>Gets the current device configuration for the device.</summary>
        /// <param name="deviceId">The unique device id.</param>
        /// <returns>An IDevice.</returns>
        IDevice DeviceConfiguration(int deviceId);

        /// <summary>Creates an adapter.</summary>
        /// <param name="name">The unique name identifying the device for discovery.</param>
        /// <returns>The new adapter.</returns>
        TAdapter CreateAdapter(string name);

        /// <summary>
        ///     Inspects the device with given protocol with the given communication settings.
        /// </summary>
        /// <param name="deviceId">The unique device id.</param>
        /// <param name="config">communication settings used to connect to the device.</param>
        /// <param name="timeout">time in milliseconds to notify of failed initialization if expired.</param>
        void Inspect(int deviceId, IComConfiguration config, int timeout);
    }
}