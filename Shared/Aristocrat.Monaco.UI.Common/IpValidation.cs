using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Aristocrat.Monaco.UI.Common
{
    /// <summary>
    /// IpValidation helper class
    /// </summary>
    public class IpValidation
    {
        private static readonly Regex ValidIpV4AddressRegex = new Regex(
            @"\b(25[0-5]|2[0-4][0-9]|[1]?[0-9][0-9]|[1-9]?[0-9]?\d)\.(25[0-5]|2[0-4][0-9]|[1]?[0-9][0-9]|[1-9]?[0-9]?\d)\.(25[0-5]|2[0-4][0-9]|[1]?[0-9][0-9]|[1-9]?[0-9]?\d)\.(25[0-5]|2[0-4][0-9]|[1]?[0-9][0-9]|[1-9]?[0-9]?\d)\b",
            RegexOptions.IgnoreCase);

        private static readonly Regex ValidIpV4SubnetMaskRegex = new Regex(
            @"^(((255\.){3}(255|254|252|248|240|224|192|128|0+))|((255\.){2}(255|254|252|248|240|224|192|128|0+)\.0)|((255\.)(255|254|252|248|240|224|192|128|0+)(\.0+){2})|((255|254|252|248|240|224|192|128|0+)(\.0+){3}))$",
            RegexOptions.IgnoreCase);

        private static readonly Dictionary<int, bool> ReservedFirstOctet = new Dictionary<int, bool>
        {
            ////String of special IPVs that we can set valid/invalid
            { 0, true },
            { 10, true },
            { 100, true },
            { 127, true },
            { 169, true },
            { 172, true },
            { 192, true },
            { 198, true },
            { 203, true },
            { 224, true },
            { 240, true },
            { 255, true }
        };

        private static readonly Dictionary<string, bool> Reserved = new Dictionary<string, bool>
        {
            ////String of special IPVs that we can set valid/invalid
            { "0.0.0.0", false },
            { "10.0.0.0", false },
            { "10.255.255.255", false },
            { "127.0.0.0", false },
            { "172.16.0.0", false },
            { "172.31.255.255", false },
            { "192.88.99.0", false },
            { "192.168.0.0", false },
            { "192.168.255.255", false },
            { "198.18.0.0", false },
            { "198.51.100.0", false },
            { "203.0.113.0", false },
            { "224.0.0.0", false },
            { "225.0.0.1", false },
            { "230.100.205.200", false },
            { "235.50.100.1", false },
            { "239.105.5.20", false },
            { "239.255.255.254", false },
            { "240.0.0.0", false }
        };

        /// <summary>
        /// IsIpV4AddressValid
        /// </summary>
        /// <param name="address"></param>
        /// <returns>bool</returns>
        public static bool IsIpV4AddressValid(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            var trimmed = address.Trim();
            if (!ValidIpV4AddressRegex.IsMatch(trimmed))
            {
                return false;
            }

            if (!IsInterNetworkAndNotReserved(trimmed))
            {
                return false;
            }

            return GetReservedValueIfExists(trimmed);
        }

        /// <summary>
        /// IsIpV4SubnetMaskValid
        /// </summary>
        /// <param name="address"></param>
        /// <returns>bool</returns>
        public static bool IsIpV4SubnetMaskValid(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return false;
            }

            var trimmed = address.Trim();
            if (!ValidIpV4SubnetMaskRegex.IsMatch(trimmed))
            {
                return false;
            }

            return GetReservedValueIfExists(trimmed);
        }

        private static bool IsInterNetworkAndNotReserved(string trimmed)
        {
            if (!IPAddress.TryParse(trimmed, out var ipAddress))
            {
                return false;
            }

            bool flag;
            
            switch (ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    if (IPAddress.IsLoopback(ipAddress))
                    {
                        flag = false;
                        break;
                    }

                    var firstOctet = ipAddress.GetAddressBytes()[0];

                    flag = !GetReservedFirstOctetIfExists(firstOctet) || IsValidReservedIpv4Usable(trimmed);
                    
                    break;

                case AddressFamily.InterNetworkV6:
                    flag = false;
                    break;

                default:
                    flag = false;
                    break;
            }

            return flag;
        }

        private static bool GetReservedFirstOctetIfExists(int firstOctet)
        {
            if (ReservedFirstOctet.TryGetValue(firstOctet, out var value))
            {
                return value;
            }
            else
            {
                return true;
            }
        }

        private static bool GetReservedValueIfExists(string ipAddress)
        {
            if (Reserved.TryGetValue(ipAddress, out var value))
            {
                return value;
            }
            else
            {
                return true;
            }
        }

        private static int IpStringToInt(string ip)
        {
            var bytes = IPAddress.Parse(ip).GetAddressBytes();
            return IPAddress.HostToNetworkOrder(BitConverter.ToInt32(bytes, 0));
        }

        private static bool IsValidReservedIpv4Usable(string ip)
        {
            try
            {
                var address = IpStringToInt(ip);

                // Loop Back IPs
                if (address >= IpStringToInt("127.0.0.0") && address <= IpStringToInt("127.255.255.255"))
                {
                    return false;
                }

                // Used for link-local addresses between two hosts on a single link when no IP address is otherwise specified,
                // such as would have normally been retrieved from a DHCP server
                if (address >= IpStringToInt("169.254.0.0") && address <= IpStringToInt("169.254.255.255"))
                {
                    return false;
                }

                // exclude 255.255.255.255
                if (address == IpStringToInt("255.255.255.255"))
                {
                    return false;
                }

                // Class A IP Check
                if (address >= IpStringToInt("10.0.0.1") && address <= IpStringToInt("10.255.255.254"))
                {
                    return true;
                }

                // Class B IP Check
                if (address >= IpStringToInt("192.168.0.1") && address <= IpStringToInt("192.168.255.254"))
                {
                    return true;
                }

                // Class C IP Check
                if (address >= IpStringToInt("172.16.0.1") && address <= IpStringToInt("172.31.255.254"))
                {
                    return true;
                }

                // Reserved for multicast
                if (address >= IpStringToInt("224.0.0.0") && address <= IpStringToInt("239.255.255.255"))
                {
                    return false;
                }

                // Reserved for multicast
                if (address >= IpStringToInt("240.0.0.0") && address <= IpStringToInt("255.255.255.254"))
                {
                    return false;
                }

                // Used for broadcast messages to the current ("this")[1]
                if (address >= IpStringToInt("0.0.0.1") && address <= IpStringToInt("0.255.255.254"))
                {
                    return true;
                }

                // Used for communications between a service provider and its subscribers when using a carrier-grade NAT[3]
                if (address >= IpStringToInt("100.64.0.1") && address <= IpStringToInt("100.127.255.254"))
                {
                    return true;
                }

                // Used for the IANA IPv4 Special Purpose Address Registry[6]
                if (address >= IpStringToInt("192.0.0.1") && address <= IpStringToInt("192.0.0.254"))
                {
                    return true;
                }

                // Assigned as "TEST-NET-1" for use in documentation and examples. It should not be used publicly.[7]
                if (address >= IpStringToInt("192.0.2.1") && address <= IpStringToInt("192.0.2.254"))
                {
                    return true;
                }

                // Used by 6to4 anycast relays (deprecated)[8]
                if (address >= IpStringToInt("192.88.99.1") && address <= IpStringToInt("192.88.99.254"))
                {
                    return true;
                }

                // Used for testing of inter-network communications between two separate subnets[9]
                if (address >= IpStringToInt("198.18.0.1") && address <= IpStringToInt("198.19.255.254"))
                {
                    return true;
                }

                // Assigned as "TEST-NET-2" for use in documentation and examples. It should not be used publicly.[7]
                if (address >= IpStringToInt("198.51.100.1") && address <= IpStringToInt("198.51.100.254"))
                {
                    return true;
                }

                // Assigned as "TEST-NET-3" for use in documentation and examples.It should not be used publicly.[7]
                if (address >= IpStringToInt("203.0.113.1") && address <= IpStringToInt("203.0.113.254"))
                {
                    return true;
                }

                return false;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
