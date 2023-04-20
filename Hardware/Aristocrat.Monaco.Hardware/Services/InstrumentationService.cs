namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Reflection;
    using System.Text;
    using Contracts.Door;
    using Kernel;
    using log4net;
    using log4net.Appender;
    using log4net.Repository.Hierarchy;

    /// <summary>
    /// </summary>
    public class InstrumentationService : IService, IDisposable
    {
        private readonly IEventBus _eventBus;
        private const string DevicesName = "Monaco-usbDevices.dmp";
        private const string Devices1Name = "Monaco-usbDevices1Log.dmp";
        private const string Devices2Name = "Monaco-usbDevices2Log.dmp";
        private const string Win32UsbHub = "Win32_USBHub";
        private const string Win32UsbControllerDevice = "Win32_USBControllerDevice";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private bool _disposed;

        public string Name => nameof(InstrumentationService);

        public ICollection<Type> ServiceTypes => new[] { typeof(InstrumentationService) };

        public InstrumentationService()
            : this(ServiceManager.GetInstance().GetService<IEventBus>())
        {
        }

        public InstrumentationService(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Initialize()
        {
            _eventBus.Subscribe<DoorOpenMeteredEvent>(this, Handle);

            if (!GetLoggerPath(out var logFolder))
            {
                return;
            }

            var logFile = $"{logFolder}\\{DevicesName}";
            if (!File.Exists(logFile))
            {
                CreateUsbDeviceLogs(logFile);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Handle(DoorOpenMeteredEvent @event)
        {
            if (@event.LogicalId == (int)DoorLogicalId.Main)
            {
                try
                {
                    if (!GetLoggerPath(out var logFolder))
                    {
                        return;
                    }

                    var logFile = $"{logFolder}\\{Devices1Name}";
                    CreateUsbDeviceLogs(logFile);

                    var apiResult = NativeMethods.ReenumerateDevices();
                    if (!apiResult)
                    {
                        Logger.Error("ReenumerateDevices Failed");
                    }

                    logFile = $"{logFolder}\\{Devices2Name}";
                    CreateUsbDeviceLogs(logFile);
                }
                catch (Exception e)
                {
                    Logger.Error($"Handle DoorOpenMeteredEvent exception {e}");
                }
            }
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private static void CreateUsbDeviceLogs(string logfile)
        {
            using var file = File.Create(logfile);
            var usbDevices = GetUSBDevices();

            var builder = new StringBuilder();

            foreach (var usbDevice in usbDevices)
            {
                foreach (var property in usbDevice)
                {
                    builder.Append($"{property.Key}: {property.Value}\n\t");
                }

                builder.Append('\n');
                var info = new UTF8Encoding(true).GetBytes(builder.ToString());
                file.Write(info, 0, info.Length);

                builder.Clear();
            }
        }

        /* Disable this, it is to invasive
        private void CreateInstrumentationLogs()
        {
            if (!GetLoggerPath(out var logFolder))
            {
                return;
            }

            var dxDiagLog = $"{logFolder}\\{DxDiagName}";

            if (!File.Exists(dxDiagLog))
            {
                Task.Run(
                    () =>
                    {
                        var DxDiagPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                            InstrumentationService.DxDiagPath);

                        var procStartInfo = new ProcessStartInfo(DxDiagPath, $"/t {dxDiagLog}");
                        using (var prc = Process.Start(procStartInfo))
                        {
                            if (prc != null)
                            {
                                prc.WaitForExit();
                                if (prc.ExitCode != 0)
                                {
                                    Logger.Error("DXDIAG failed with exit code " + prc.ExitCode);
                                }
                            }
                        }
                    });
            }

            var msInfo32Log = $"{logFolder}\\{MsInfo32Name}";

            if (!File.Exists(msInfo32Log))
            {
                Task.Run(
                    () =>
                    {
                        var msinfo32Path = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                            MsInfo32Path);

                        var procStartInfo = new ProcessStartInfo(msinfo32Path, $"/nfo {msInfo32Log}");
                        using (var prc = Process.Start(procStartInfo))
                        {
                            if (prc != null)
                            {
                                // This window doesn't have the needed styles to allow hidden to work
                                // So, move this offscreen to allow it to run to completion
                                // needs to wait for the process to create the windows
                                Thread.Sleep(500);

                                foreach (var handle in NativeMethods.EnumerateProcessWindowHandles(prc.Id))
                                {
                                    var result = NativeMethods.MoveWindow(handle);

                                    if (result)
                                    {
                                        break;
                                    }
                                }

                                prc.WaitForExit();
                                if (prc.ExitCode != 0)
                                {
                                    Logger.Error("Msinfo32 failed with exit code " + prc.ExitCode);
                                }
                            }
                        }
                    });
            }
        }*/

        private static bool GetLoggerPath(out string logFolder)
        {
            logFolder = string.Empty;
            var rootAppender = ((Hierarchy)LogManager.GetRepository())
                .Root.Appenders.OfType<FileAppender>()
                .FirstOrDefault();

            // we need to write these files to the log folder
            if (rootAppender == null)
            {
                return false;
            }

            logFolder = Path.GetDirectoryName(rootAppender.File);
            return true;
        }

        private static List<Dictionary<string, object>> GetUSBDevices()
        {
            var devices = new List<Dictionary<string, object>>();

            using (var searcher = new ManagementObjectSearcher($"Select * From {Win32UsbHub}"))
            {
                using var collection = searcher.Get();
                GetDeviceProperties(collection);
            }

            using (var searcher =
                new ManagementObjectSearcher(
                    "root\\CIMV2",
                    $"SELECT * FROM {Win32UsbControllerDevice}"))
            {
                using var collection = searcher.Get();
                GetDeviceProperties(collection);
            }

            return devices;

            void GetDeviceProperties(ManagementObjectCollection collectionSet)
            {
                foreach (var device in collectionSet)
                {
                    var deviceProperties = new Dictionary<string, object>();

                    devices.Add(deviceProperties);
                    deviceProperties["ClassName"] = device.ClassPath.ClassName;
                    deviceProperties["Path"] = device.ClassPath.Path;

                    AddProperties(device.Properties);
                    AddProperties(device.SystemProperties);
                    AddQualifiers(device.Qualifiers, string.Empty);

                    void AddQualifiers(QualifierDataCollection qualifiers, string preKey)
                    {

                        foreach (var qualifier in qualifiers)
                        {
                            deviceProperties[$"{preKey}{qualifier.Name}"] = qualifier.Value;
                        }
                    }

                    void AddProperties(PropertyDataCollection properties)
                    {
                        foreach (var property in properties)
                        {
                            deviceProperties[property.Name] = property.Value;

                            if (property.IsArray)
                            {
                                AddQualifiers(property.Qualifiers, "\t");
                            }
                        }
                    }
                }
            }
        }
    }
}
