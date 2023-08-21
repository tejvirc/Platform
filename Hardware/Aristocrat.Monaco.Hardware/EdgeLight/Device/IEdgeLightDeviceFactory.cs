namespace Aristocrat.Monaco.Hardware.EdgeLight.Device
{
    using System.Collections.Generic;
    using Contracts;
    using Kernel;

    /// <summary>
    ///     A factory for creating and getting the current edgelight devices
    /// </summary>
    public interface IEdgeLightDeviceFactory : IService
    {
        /// <summary>
        ///     Gets the collection of edge light devices
        /// </summary>
        /// <returns>The list of edgelight devices</returns>
        IEnumerable<IEdgeLightDevice> GetDevices();
    }
}