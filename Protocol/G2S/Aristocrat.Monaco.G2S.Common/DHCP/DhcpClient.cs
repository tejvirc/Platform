namespace Aristocrat.Monaco.G2S.Common.DHCP
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Text;
    using Application.Contracts;
    using Kernel;
    using log4net;

    /// <summary>
    ///     DHCP client implementation
    /// </summary>
    public class DhcpClient : IDhcpClient, IService
    {
        private const string G2SPrefix = @"g2s";
        private const string GsaPrefix = @"gsa";
        private const string Option43CommandLineKey = "forceDhcpOption43Response";
        private const int Timeout = 15000; // It's in milliseconds

        // ReSharper disable once StringLiteralTypo
        private static readonly string ClientInfo = $@"{GsaPrefix}{G2SPrefix}EGMATI";
        private static readonly ILog Logger =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly Dictionary<string, string> SampleOption43Values = new()
        {
            {"vltSampleValue","g2sCC=shs:10.3.0.70:8443//g2s/services/G2SAPIService+1|gsaCS=tou:10.3.2.246/ocsp|gsaCM=tsu:10.3.2.246/certsrv/mscep/^c=gsaCA|gsaOO=20160|GTKkl=4096"},
            {"vertexSampleValue","g2sCC=shs:192.168.50.2:9091/g2shost+1"},
        };
        private string _options;

        /// <inheritdoc />
        public string GetVendorSpecificInformation()
        {
            if (!string.IsNullOrEmpty(_options))
            {
                return _options;
            }

            try
            {
                var networkInterface = NetworkInterfaceInfo.DefaultNetworkInterface;

                if (networkInterface == null)
                {
                    Logger.Info("Failed to get network interface");

                    return string.Empty;
                }

                if (!networkInterface.GetIPProperties().DhcpServerAddresses.Any())
                {
                    Logger.Info("No DHCP servers for the available NIC");

                    return string.Empty;
                }

                Logger.Debug("Initiating request for DHCP options");

                _options = GetVendorSpecificOptions(networkInterface);

                Logger.Info($"DHCP Options: {_options}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return _options;
        }

        /// <inheritdoc />
        public string Name => typeof(DhcpClient).ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IDhcpClient) };

        /// <inheritdoc />
        public void Initialize()
        {
#if !(RETAIL)
            var propertiesManager = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var overrideOption43CommandLineValue =
                (string)propertiesManager.GetProperty(Option43CommandLineKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(overrideOption43CommandLineValue))
            {
                //if requesting a predefined sample, use it. otherwise use the provided value directly.
                if (!SampleOption43Values.TryGetValue(overrideOption43CommandLineValue, out _options))
                {
                    _options = overrideOption43CommandLineValue;
                }

                Logger.Debug($"Using Option 43 value from command line- {_options}");
            }
#endif
        }

        private static string GetVendorSpecificOptions(NetworkInterface networkInterface)
        {
            const int bufferSize = 1024;

            var sessionId = (int)DateTime.UtcNow.Ticks;

            using (var dhcpClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                dhcpClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                dhcpClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                dhcpClientSocket.ReceiveTimeout = Timeout;
                dhcpClientSocket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), DhcpConstants.DhcpClientPort));

                var physicalAddress = networkInterface.GetPhysicalAddress().GetAddressBytes();

                var discoverMessage = new DhcpMessage
                {
                    SessionId = sessionId,
                    Operation = DhcpOperation.BootRequest,
                    Hardware = HardwareType.Ethernet,
                    Flags = 128,
                    ClientHardwareAddress = physicalAddress
                };

                var address = networkInterface.GetIPProperties().UnicastAddresses
                    .FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                if (address != null)
                {
                    discoverMessage.ClientAddress = address.Address.GetAddressBytes();
                }

                discoverMessage.AddOption(DhcpOption.DhcpMessageType, (byte)DhcpMessageType.Inform);

                var clientId = new byte[physicalAddress.Length + 1];
                clientId[0] = 1;
                physicalAddress.CopyTo(clientId, 1);
                discoverMessage.AddOption(DhcpOption.Hostname, Encoding.ASCII.GetBytes(Environment.MachineName));
                discoverMessage.AddOption(DhcpOption.ClassId, Encoding.ASCII.GetBytes(ClientInfo));
                discoverMessage.AddOption(DhcpOption.ClientId, clientId);
                discoverMessage.AddOption(DhcpOption.ParameterList, (byte)DhcpOption.VendorSpecificInfo);

                Logger.Debug($"Sending request for {networkInterface.GetPhysicalAddress()} - {ClientInfo}");

                dhcpClientSocket.SendTo(discoverMessage.ToArray(), new IPEndPoint(IPAddress.Broadcast, DhcpConstants.DhcpServerPort));

                var buffer = new byte[bufferSize];
                var len = dhcpClientSocket.Receive(buffer);

                Logger.Debug($"Received {len} byte response");

                if (len > 0)
                {
                    var messageData = new byte[len];
                    Array.Copy(buffer, messageData, Math.Min(len, buffer.Length));
                    var responseMessage = new DhcpMessage(messageData);

                    Logger.Debug($"DHCP message - {Encoding.ASCII.GetString(messageData)}");

                    var data = responseMessage.GetOptionData(DhcpOption.VendorSpecificInfo);
                    if (data != null)
                    {
                        var result = Encoding.ASCII.GetString(data);

                        Logger.Debug($"Option 43 value - {result}");

                        if (result.Length > 2)
                        {
                            if (IsValid(result))
                            {
                                return result;
                            }

                            // Skip the first 2 bytes. Byte 1 is the option. Byte 2 is the length
                            var options = result.Substring(2);

                            Logger.Debug($"Option 43 info {Convert.ToInt32(data[0])} - ({Convert.ToInt32(data[1])}): {options}");

                            if (IsValid(options))
                            {
                                return options;
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        private static bool IsValid(string options)
        {
            return options.StartsWith(G2SPrefix, StringComparison.InvariantCulture)
                   || options.StartsWith(GsaPrefix, StringComparison.InvariantCulture);
        }
    }
}