namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Contracts.Communicator;
    using Contracts.SharedDevice;
    using Hardware.Contracts.SerialPorts;
    using log4net;
    using Serial.NoteAcceptor;
    using Serial.Printer;
    using Serial.Protocols;
    using Serial.Reel;

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
            try
            {
                Logger.Debug("Search");
                if (!supportedDevices.Any())
                {
                    return null;
                }

                var deviceType = (DeviceType)Enum.Parse(typeof(DeviceType), supportedDevices.First().Type);
                Logger.Debug($"Search port {port} for device {deviceType}");

                var protocolTypes = new List<Type>();
                var allTypes = typeof(SerialDeviceProtocol).Assembly.GetTypes().ToList();

                switch (deviceType)
                {
                    case DeviceType.NoteAcceptor:
                        protocolTypes.AddRange(allTypes.Where(t => t.IsSubclassOf(typeof(SerialNoteAcceptor)) && !t.IsAbstract));
                        break;

                    case DeviceType.Printer:
                        protocolTypes.AddRange(allTypes.Where(t => t.IsSubclassOf(typeof(SerialPrinter)) && !t.IsAbstract));
                        break;

                    case DeviceType.ReelController:
                        protocolTypes.AddRange(allTypes.Where(t => t.IsSubclassOf(typeof(SerialReelController)) && !t.IsAbstract));
                        break;

                    default:
                        // Monaco has no serial IdReader protocols
                        break;
                }

                Logger.Debug($"{protocolTypes.Count} protocol types");

                foreach (var protocolType in protocolTypes)
                {
                    // Use this protocol
                    var protocol = (SerialDeviceProtocol)protocolType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                        null, Type.EmptyTypes, null)?.Invoke(new object[] { });
                    if (protocol is null)
                    {
                        Logger.Debug($"Couldn't find constructor() for {protocolType.Name}");
                        continue;
                    }

                    var protocolDevices = supportedDevices.Where
                        (d => protocol.GetType().Name.ToLower().Contains(d.Protocol.ToLower()) || d.Protocol.ToLower().Contains(protocol.GetType().Name.ToLower()))
                        .ToList();
                    Logger.Debug($"Protocol {protocolType} used by {protocolDevices.Count} devices");

                    foreach (var protocolDevice in protocolDevices)
                    {
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

                        Logger.Debug($"Configure? name={protocol.Name} setup={GetDeviceDescription(protocolDevice)}");
                        protocol.Device = new Device(null, null);
                        protocol.Manufacturer = string.Empty;
                        if (protocol.Configure(config) && !string.IsNullOrEmpty(protocol.Manufacturer))
                        {
                            // Find device to match the results
                            Logger.Debug($"Found name={protocol.Name} mfg={protocol.Manufacturer} model={protocol.Model} proto={protocol.Protocol}");
                            protocol.Dispose();
                            return supportedDevices.FirstOrDefault(d => d.Name.StartsWith(protocol.Manufacturer));
                        }
                    }

                    protocol.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex.Message} : {ex.StackTrace}");
                throw;
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
    }
}
