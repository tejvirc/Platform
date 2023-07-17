namespace Aristocrat.Monaco.Application.Contracts.Detection
{
    using System.Collections.Generic;
    using Hardware.Contracts.SharedDevice;
    using Kernel;

    /// <summary>
    ///     Device detection interface
    /// </summary>
    public interface IDeviceDetection : IService
    {
        /// <summary>
        ///     Begin detection
        /// </summary>
        /// <param name="devices">Which devices to try to detect</param>
        void BeginDetection(IEnumerable<DeviceType> devices);

        /// <summary>
        ///     Cancel detection
        /// </summary>
        void CancelDetection();
    }
}