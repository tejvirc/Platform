namespace Aristocrat.Monaco.G2S
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;

    /// <summary>
    ///     Provides a mechanism to create a device
    /// </summary>
    public interface IDeviceFactory
    {
        /// <summary>
        ///     Creates a device using the provided callback and registers the device with the specified host.
        /// </summary>
        /// <param name="host">The host</param>
        /// <param name="createDevice">The callback invoked to create the device.</param>
        /// <returns>A device.</returns>
        IDevice Create(IHost host, Func<ClientDeviceBase> createDevice);

        /// <summary>
        ///     Creates a device using the provided callback and registers the device with the specified host.
        /// </summary>
        /// <param name="host">The host</param>
        /// <param name="guests">A list of guests for the device.</param>
        /// <param name="createDevice">The callback invoked to create the device.</param>
        /// <returns>A device.</returns>
        IDevice Create(IHost host, IEnumerable<IHost> guests, Func<ClientDeviceBase> createDevice);
    }
}