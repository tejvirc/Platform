namespace Aristocrat.Monaco.G2S.Meters
{
    using System.Collections.Generic;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;

    /// <summary>
    ///     Provides a mechanism to gather meters for a device.
    /// </summary>
    /// <typeparam name="TDevice">The device type</typeparam>
    public interface IMeterAggregator<in TDevice>
        where TDevice : IDevice
    {
        /// <summary>
        ///     Gets the meters for the device.
        /// </summary>
        /// <param name="device">The device</param>
        /// <param name="includeMeters">A list of meters to include.</param>
        /// <returns>A collection of meters.</returns>
        IEnumerable<meterInfo> GetMeters(TDevice device, params string[] includeMeters);
    }
}