namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Contracts.Communicator;
    using Contracts.SharedDevice;
    using Hardware.Contracts.SerialPorts;
    using log4net;
    using Serial.Protocols;

    public class SerialDeviceSearcher : ISerialDeviceSearcher
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        public string Name => nameof(SerialDeviceSearcher);

        public ICollection<Type> ServiceTypes => new List<Type> { typeof(ISerialDeviceSearcher) };

        public void Initialize()
        {
            Logger.Debug("Initialize");
        }

        public SupportedDevicesDevice Search(string port, List<SupportedDevicesDevice> supportedDevices, CancellationToken token)
        {
            if (!supportedDevices.Any())
            {
                return null;
            }

            var deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), supportedDevices.First().Type);
            Logger.Debug($"Search port {port} for deviceType {deviceType}");

            var protocolTypes = new List<Type>();
            var allTypes = typeof(SerialDeviceProtocol).Assembly.GetTypes().ToList();
            foreach (var type in allTypes)
            {
                if (type.GetCustomAttributes(typeof(SearchableSerialProtocolAttribute), false).Length > 0)
                {
                    var attr = type.GetCustomAttribute(typeof(SearchableSerialProtocolAttribute)) as SearchableSerialProtocolAttribute;
                    if (attr.DeviceType == deviceType)
                    {
                        protocolTypes.Add(type);
                    }
                }
            }

            Logger.Debug($"({deviceType}) {protocolTypes.Count} protocol types");

            foreach (var protocolType in protocolTypes)
            {
                var protocolDevices = supportedDevices.Where
                    (d => protocolType.Name.ToLower().Contains(d.Protocol.ToLower()) || d.Protocol.ToLower().Contains(protocolType.Name.ToLower()))
                    .ToList();
                Logger.Debug($"({deviceType}) Protocol {protocolType} used by {protocolDevices.Count} devices");

                foreach (var protocolDevice in protocolDevices)
                {
                    SerialDeviceProtocol protocol = null;

                    try
                    {
                        // Use this protocol
                        protocol = (SerialDeviceProtocol)protocolType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                            null, Type.EmptyTypes, null)?.Invoke(new object[] { });
                        if (protocol is null)
                        {
                            Logger.Debug($"({deviceType}) Couldn't find constructor() for {protocolType.Name}");
                            continue;
                        }

                        // Use this IComConfiguration
                        var config = new ComConfiguration
                        {
                            PortName = port,
                            BaudRate = int.Parse(GetDeviceItemByName(protocolDevice, ItemsChoiceType.BaudRate)),
                            DataBits = int.Parse(GetDeviceItemByName(protocolDevice, ItemsChoiceType.DataBits)),
                            StopBits = (StopBits)Enum.Parse(typeof(StopBits), GetDeviceItemByName(protocolDevice, ItemsChoiceType.StopBits)),
                            Parity = (Parity)Enum.Parse(typeof(Parity), GetDeviceItemByName(protocolDevice, ItemsChoiceType.Parity)),
                            Handshake = (Handshake)Enum.Parse(typeof(Handshake), GetDeviceItemByName(protocolDevice, ItemsChoiceType.Handshake))
                        };

                        Logger.Debug($"({deviceType}) Configure? {protocolDevice.Name} setup=({GetDeviceDescription(protocolDevice)})");
                        protocol.Device = new Device(null, null);
                        protocol.Manufacturer = string.Empty;

                        if (protocol.Configure(config))
                        {
                            var delay = (protocolType.GetCustomAttribute(typeof(SearchableSerialProtocolAttribute)) as SearchableSerialProtocolAttribute).MaxWaitForResponse;
                            while (delay > 0 && string.IsNullOrEmpty(protocol.FirmwareVersion))
                            {
                                const int millisecondsPerSecond = 1000;

                                delay -= millisecondsPerSecond;
                                Task.Delay(millisecondsPerSecond).Wait();
                            }

                            // Find device to match the results
                            Logger.Debug($"({deviceType}) Tried {protocolDevice.Name} mfg={protocol.Manufacturer} model={protocol.Model} proto={protocol.Protocol} firmware={protocol.FirmwareVersion}");
                            if (DoesDeviceFirmwareMatch(protocol.FirmwareVersion, GetDeviceItemByName(protocolDevice, ItemsChoiceType.FirmwareVersionRegex)))
                            {
                                return protocolDevice;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"({deviceType}) ({port}) {ex.Message} : {ex.StackTrace}");
                        throw;
                    }
                    finally
                    {
                        // This executes whether we continue the loop, return, or catch an exception.
                        protocol?.Dispose();
                    }
                }
            }

            return null;
        }

        private string GetDeviceItemByName(SupportedDevicesDevice device, ItemsChoiceType name)
        {
            for (var index = 0; index < device.Items.Length; index++)
            {
                if (device.ItemsElementName[index] == name)
                {
                    return device.Items[index];
                }
            }

            return string.Empty;
        }

        private string GetDeviceDescription(SupportedDevicesDevice x)
        {
            return string.Join(" ", new[] {
                GetDeviceItemByName(x, ItemsChoiceType.BaudRate),
                GetDeviceItemByName(x, ItemsChoiceType.DataBits),
                GetDeviceItemByName(x, ItemsChoiceType.StopBits),
                GetDeviceItemByName(x, ItemsChoiceType.Parity),
                GetDeviceItemByName(x, ItemsChoiceType.Handshake) });
        }

        private bool DoesDeviceFirmwareMatch(string reportedFirmware, string matchRegex)
        {
            if (string.IsNullOrEmpty(reportedFirmware))
            {
                Logger.Debug($"Compare fw '{reportedFirmware}' to regex '{matchRegex}': False, empty reported");
                return false;
            }

            if (string.IsNullOrEmpty(matchRegex))
            {
                Logger.Debug($"Compare fw '{reportedFirmware}' to regex '{matchRegex}': True, empty matchregex");
                return true;
            }

            var isMatch = new Regex(matchRegex).IsMatch(reportedFirmware);
            Logger.Debug($"Compare fw '{reportedFirmware}' to regex '{matchRegex}': {isMatch}");
            return isMatch;
        }
    }
}
