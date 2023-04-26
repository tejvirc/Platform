namespace Aristocrat.Monaco.Hardware.Services
{
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Management;
    //using Cabinet.Contracts;
    using Microsoft.Win32;

    public class SerialPortEnumerator
    {
        private const string ComText = "COM";
        private const string Rs232SerialPort = "RS232 Serial Port";
        private const string ModemDevice = "Modem Device";
        private const string PnpDeviceIdString = "PNPDeviceId";
        private const string DeviceIdString = "DeviceId";
        private const string ProviderTypeString = "ProviderType";
        private const string WmiClassString = "Win32_SerialPort";
        private const string WmiQueryScope = "root\\CIMV2";
        private const string RegistryQuery = "SYSTEM\\ControlSet001\\Enum\\";
        private const string RegistryAddressKey = "Address";

        private readonly string _wmiQueryString =
            $"Select {PnpDeviceIdString}, {DeviceIdString}, {ProviderTypeString} from {WmiClassString}";

        public virtual IEnumerable<SerialPortInfo> EnumerateSerialPorts()
        {
            var serialPorts = new List<SerialPortInfo>();
            //switch (HardwareFamilyIdentifier.Identify())
            //{
            //    case HardwareFamily.Unknown:
                    PopulateSerialPortsOnUnknownCabinet(serialPorts);
            //        break;

            //    default:
            //        PopulateSerialPortsOnEgm(serialPorts);
            //        break;
            //}

            return serialPorts;
        }

        private void PopulateSerialPortsOnUnknownCabinet(List<SerialPortInfo> serialPorts)
        {
            serialPorts.AddRange(
                SerialPort.GetPortNames().Select(
                    portName => new SerialPortInfo
                    {
                        Address = int.TryParse(portName.Substring(ComText.Length), out var index)
                            ? index
                            : 0,
                        SerialPortType = SerialPortType.Rs232,
                        PhysicalPortName = portName
                    }));
        }

        private void PopulateSerialPortsOnEgm(ICollection<SerialPortInfo> serialPorts)
        {
            // Fetch all Serial ports from Wmi
            var searcher = new ManagementObjectSearcher(
                WmiQueryScope,
                _wmiQueryString);

            foreach (var o in searcher.Get())
            {
                var queryObj = (ManagementObject)o;
                string pnpDeviceId = (string)queryObj[PnpDeviceIdString],
                    deviceId = (string)queryObj[DeviceIdString],
                    providerType = (string)queryObj[ProviderTypeString];

                if (providerType == null)
                {
                    continue;
                }

                // Fetch serial port address from registry.
                using (var reg = Registry.LocalMachine.OpenSubKey(RegistryQuery + pnpDeviceId))
                {
                    // address from registry
                    var value = reg?.GetValue(RegistryAddressKey);

                    if (value == null)
                    {
                        continue;
                    }

                    var id = (int)value;
                    var type = SerialPortType.Unknown;

                    if (providerType.Equals(ModemDevice))
                    {
                        type = SerialPortType.Usb;
                    }
                    else if (providerType.Equals(Rs232SerialPort))
                    {
                        type = SerialPortType.Rs232;
                    }

                    serialPorts.Add(
                        new SerialPortInfo
                        {
                            Address = id,
                            SerialPortType = type,
                            PhysicalPortName = deviceId
                        });
                }
            }
        }
    }
}