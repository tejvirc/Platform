namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Contracts.Door;
    using Kernel;
    using log4net;
    using log4net.Appender;
    using log4net.Repository.Hierarchy;
    using NativeUsb;

    /// <summary>
    ///     This class is used for logging all the usb devices attached into Monaco and their properties.
    ///     This is useful for debugging PNP devices.  This class will create usb log files for the currently connected
    ///     devices and their properties.  These dump files can be opened via any text editor and read.
    /// </summary>
    public class InstrumentationService : IService, IDisposable
    {
        private const string DevicesName = "Monaco-usbDevices.dmp";
        private const string Devices1Name = "Monaco-usbDevices1Log.dmp";
        private const string Devices2Name = "Monaco-usbDevices2Log.dmp";
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);
        private readonly IEventBus _eventBus;

        private bool _disposed;

        public InstrumentationService(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => nameof(InstrumentationService);

        public ICollection<Type> ServiceTypes => new[] { typeof(InstrumentationService) };

        public void Initialize()
        {
            _eventBus.Subscribe<DoorOpenMeteredEvent>(this, Handle, evt => evt.LogicalId == (int)DoorLogicalId.Main);
            if (!GetLoggerPath(out var logFolder))
            {
                return;
            }

            var logFile = $"{logFolder}\\{DevicesName}";
            if (!File.Exists(logFile))
            {
                UsbDeviceLogger.CreateUsbDeviceLogs(logFile);
            }
        }

        private static void Handle(DoorOpenMeteredEvent @event)
        {
            try
            {
                if (!GetLoggerPath(out var logFolder))
                {
                    return;
                }

                var logFile = $"{logFolder}\\{Devices1Name}";
                UsbDeviceLogger.CreateUsbDeviceLogs(logFile);

                var apiResult = UsbDeviceLogger.ReEnumerateUsbDevices();
                if (!apiResult)
                {
                    Logger.Error("ReenumerateDevices Failed");
                }

                logFile = $"{logFolder}\\{Devices2Name}";
                UsbDeviceLogger.CreateUsbDeviceLogs(logFile);
            }
            catch (Exception e)
            {
                Logger.Error($"Handle DoorOpenMeteredEvent exception {e}");
            }
        }

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
    }
}