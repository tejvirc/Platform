namespace Aristocrat.Monaco.Application.Contracts
{
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    /// <summary>
    ///     Provides easily-consumable information about the network interface(s) on the machine.
    /// </summary>
    public static class NetworkInterfaceInfo
    {
        /// <summary>
        ///     Gets the physical (MAC) address of the first available NIC on the machine.
        /// </summary>
        public static bool DefaultIsUp =>
            NetworkInterface.GetAllNetworkInterfaces()
                .Any(nic => nic.OperationalStatus == OperationalStatus.Up && IsSupported(nic));

        /// <summary>
        ///     Gets the physical (MAC) address of the first available NIC on the machine.
        /// </summary>
        public static NetworkInterface DefaultNetworkInterface =>
            NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && IsSupported(nic))
                .DefaultIfEmpty(FallBackNetworkInterface())
                .FirstOrDefault();

        /// <summary>
        ///     Gets the physical (MAC) address of the first available NIC on the machine.
        /// </summary>
        public static string DefaultPhysicalAddress =>
            NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && IsSupported(nic))
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .DefaultIfEmpty(FallBack())
                .FirstOrDefault();

        /// <summary>
        ///     Gets the default IpAddress
        /// </summary>
        /// <remarks>
        ///     This is simply grabbing the first enabled IP address. This may not always work if there are mutliple NICs
        /// </remarks>
        public static IPAddress DefaultIpAddress =>
            Dns.GetHostEntry(string.Empty).AddressList?
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

        private static string FallBack()
        {
            // This favors LAN2 over LAN1 since the board has the MAC address of LAN2 stamped on it
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(IsSupported)
                .OrderByDescending(nic => nic.Name)
                .Select(nic => nic.GetPhysicalAddress().ToString())
                .DefaultIfEmpty(PhysicalAddress.None.ToString())
                .FirstOrDefault();
        }

        private static NetworkInterface FallBackNetworkInterface()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(IsSupported)
                .OrderBy(nic => nic.Name)
                .FirstOrDefault();
        }

        private static bool IsSupported(NetworkInterface nic)
        {
            return nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                   nic.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet ||
                   nic.NetworkInterfaceType == NetworkInterfaceType.FastEthernetT ||
                   nic.NetworkInterfaceType == NetworkInterfaceType.FastEthernetFx;
        }
    }
}
