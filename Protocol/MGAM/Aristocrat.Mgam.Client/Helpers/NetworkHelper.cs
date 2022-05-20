namespace Aristocrat.Mgam.Client.Helpers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    /// <summary>
    ///     Network helper functions and method extensions.
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        public static int GetNextUdpPort()
        {
            return Enumerable.Range(5000, 6000).First(
                p => IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().All(l => l.Port != p));
        }

        /// <summary>
        ///     
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
