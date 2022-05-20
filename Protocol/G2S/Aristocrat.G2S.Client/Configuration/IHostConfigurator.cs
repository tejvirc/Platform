namespace Aristocrat.G2S.Client.Configuration
{
    using System.Collections.Generic;
    using Devices;

    /// <summary>
    ///     Provides a mechanism to configure a G2S host.
    /// </summary>
    public interface IHostConfigurator
    {
        /// <summary>
        ///     Applies the specified permissions to the host.
        /// </summary>
        /// <param name="hostId">The host identifier.</param>
        /// <param name="owned">The list of devices for which the host is the owner.</param>
        /// <param name="config">The list of devices for which the host is the configurator.</param>
        /// <param name="guest">The list of devices for which the host is a registered guest.</param>
        /// <returns>the affected devices.</returns>
        IEnumerable<IDevice> ApplyHostPermissions(
            int hostId,
            IEnumerable<OwnedDevice> owned,
            IEnumerable<IDevice> config,
            IEnumerable<IDevice> guest);
    }
}