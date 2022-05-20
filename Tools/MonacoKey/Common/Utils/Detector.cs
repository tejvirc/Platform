namespace Common.Utils
{
    using Common.Models;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Threading;
    using System.Xml.Serialization;

    public static class Detector
    {
        private static object DetectionLocker = new object();
        public static ILog Log;

        public static List<Command> ParseCommands()
        {
            Log.Debug("Parsing Commands from embedded resources...");

            List<Command> commands = new List<Command> { };
            var assembly = Assembly.GetExecutingAssembly();
            var cdResource = "Common.CommandData.xml";

            using (Stream cdStream = assembly.GetManifestResourceStream(cdResource))
            {
                using (StreamReader cdReader = new StreamReader(cdStream))
                {
                    XmlSerializer cdSerializer = new XmlSerializer(typeof(CommandData));
                    CommandData cd = (CommandData)cdSerializer.Deserialize(cdReader);

                    foreach (Command command in cd.Commands)
                    {
                        var scriptResource = "Common.Scripts." + command.ScriptFileName;
                        using (Stream stream = assembly.GetManifestResourceStream(scriptResource))
                        {
                            if (stream == null)
                            {
                                Log.Info($"For command {command.Name}, failed to find script resource: " + command.ScriptFileName + "   Thus, excluding the command from available commands.");
                            }
                            else
                            {
                                using (StreamReader reader = new StreamReader(stream))
                                {
                                    command.Script = reader.ReadToEnd();
                                }
                                commands.Add(command);
                            }
                        }
                    }
                }
            }

            Log.Debug($"Found {commands.Count} defined commands");
            foreach (Command com in commands)
                Log.Debug("Command: " + com.Name + "          " + com.ScriptFileName);

            return commands;
        }
        public static List<USBKey> DetectAndValidateUsbs(List<Command> allCommands, RsaService rsaService)
        {
            Log.Debug("Detecting USB drives...");
            Thread.Sleep(500); // Windows takes a little time to get all it's data ready, this pause helps detection appear smoother

            lock (DetectionLocker)
            {
                List<USBKey> keys = new List<USBKey> { };

                foreach (ManagementObject diskDrive in new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive").Get())
                {
                    string diskMediaType;
                    uint diskIndex;
                    string diskInterfaceType;
                    string diskDeviceId;
                    string diskModel;
                    string diskPNPID;
                    string diskSerialNumber; //  Not unique to each physical usb. Same for all usb's of the same model.
                    ulong diskSize;

                    try
                    {
                        diskMediaType = (string)diskDrive["MediaType"];
                        diskIndex = (uint)diskDrive["Index"];
                        diskInterfaceType = (string)diskDrive["InterfaceType"];
                        diskDeviceId = (string)diskDrive["DeviceId"];
                        diskModel = (string)diskDrive["Model"];
                        diskPNPID = (string)diskDrive["PNPDeviceID"];
                        diskSerialNumber = (string)diskDrive["SerialNumber"]; //  Not unique to each physical usb. Same for all usb's of the same model.
                        diskSize = (ulong)(diskDrive["Size"] ?? (ulong)0);
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                        Log.Debug(e.StackTrace);
                        continue;
                    }

                    // safety first kid, you'll shoot your eye out
                    if (diskIndex == 0)
                        continue;
                    if (diskIndex == 4294967295) // https://msdn.microsoft.com/en-us/ie/aa394132(v=vs.94) indicates given drive does not map to a physical drive
                        continue;
                    if (diskMediaType != "Removable Media" && diskMediaType != null) // on a RAW partitioned USB, diskMediaType = null
                        continue;
                    if (diskInterfaceType != "USB")
                        continue;

                    keys.Add(new USBKey(diskIndex, diskDeviceId, diskModel, diskSerialNumber, diskPNPID, diskSize, Log));
                }

                Log.Debug($"Found {keys.Count} usb drives. Starting validation check...");

                foreach(USBKey key in keys)
                    key.ComputePartitionData();

                foreach (USBKey datKey in keys)
                    Validator.Validate(datKey, allCommands, rsaService);

                return keys.OrderBy(key => key.ProductionReady).ThenBy(key => key.DiskIndex).ToList();
            }
        }
    }
}
