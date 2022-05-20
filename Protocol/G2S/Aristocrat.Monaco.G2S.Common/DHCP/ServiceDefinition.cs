namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Contains DHCP vendor service information.
    /// </summary>
    public class ServiceDefinition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ServiceDefinition" /> class.
        /// </summary>
        /// <param name="name">The service name.</param>
        /// <param name="address">Address of the host</param>
        /// <param name="hostId">The host id.</param>
        /// <param name="serviceParameters">The service parameters.</param>
        public ServiceDefinition(
            string name,
            Uri address,
            int hostId,
            Dictionary<string, string> serviceParameters)
        {
            Name = name;
            ServiceParameters = serviceParameters;
            HostId = hostId;
            Address = address;
        }

        /// <summary>
        ///     Gets a name of the service.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets a a parameter found in one or more of the GSA application protocols,
        ///     specifically G2S and S2S.
        /// </summary>
        public int HostId { get; }

        /// <summary>
        ///     Gets an Uri address of the service.
        /// </summary>
        public Uri Address { get; }

        /// <summary>
        ///     Gets a service parameters.
        /// </summary>
        public Dictionary<string, string> ServiceParameters { get; }
    }
}