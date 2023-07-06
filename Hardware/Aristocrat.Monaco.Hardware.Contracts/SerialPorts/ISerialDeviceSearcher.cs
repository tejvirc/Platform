namespace Aristocrat.Monaco.Hardware.Contracts.SerialPorts
{
    using System.Collections.Generic;
    using System.Threading;
    using Kernel;

    /// <summary>
    ///     Define interface to search for serial devices
    /// </summary>
    public interface ISerialDeviceSearcher : IService
    {
        /// <summary>
        ///     Search for a device of the specified type and port.
        /// </summary>
        /// <param name="port">COM port to search on</param>
        /// <param name="supportedDevices">supported devices</param>
        /// <param name="token">cancellation token</param>
        SupportedDevicesDevice Search(string port, List<SupportedDevicesDevice> supportedDevices, CancellationToken token);
    }
}
