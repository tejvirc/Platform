namespace Aristocrat.Monaco.Application.Contracts
{
    using System.Net.NetworkInformation;
    using System.Threading.Tasks;

    /// <summary>
    ///     Describes the network information within the network service
    /// </summary>
    public struct NetworkInfo
    {
        /// <summary>
        ///     True if DHCP is enabled
        /// </summary>
        public bool DhcpEnabled;

        /// <summary>
        ///     The IP address
        /// </summary>
        public string IpAddress;

        /// <summary>
        ///     The Subnet Mask
        /// </summary>
        public string SubnetMask;

        /// <summary>
        ///     The Default Gateway
        /// </summary>
        public string DefaultGateway;

        /// <summary>
        ///     The preferred DNS server
        /// </summary>
        public string PreferredDnsServer;

        /// <summary>
        ///     The alternate DNS server
        /// </summary>
        public string AlternateDnsServer;
    }

    /// <summary>
    ///     Provides a mechanism to interact with the network adapters
    /// </summary>
    public interface INetworkService
    {
        /// <summary>
        ///     Get the network information for the provided adapter
        /// </summary>
        /// <returns></returns>
        NetworkInfo GetNetworkInfo();

        /// <summary>
        ///     Sets the network info for the provided adapter
        /// </summary>
        /// <param name="networkInfo"></param>
        /// <returns></returns>
        bool SetNetworkInfo(NetworkInfo networkInfo);

        /// <summary>
        ///     Execute Ping to detect network connection
        /// </summary>
        /// <param name="addr">the IP address or the name of the target machine</param>
        /// <returns>the result of Ping action</returns>
        Task<PingReply> PingAsync(string addr);
    }
}