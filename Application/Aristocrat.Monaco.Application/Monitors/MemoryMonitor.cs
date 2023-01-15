namespace Aristocrat.Monaco.Application.Monitors
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Contracts;
    using Contracts.Localization;
    using Kernel;
    using log4net;
    using Aristocrat.Monaco.Localization.Properties;
    using Kernel.Contracts.MessageDisplay;

    public class MemoryMonitor : IService, IDisposable
    {
        private bool _disposed;
        private ulong _memoryLeftThreshold; 

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly Guid LockupId = ApplicationConstants.MemoryBelowThresholdDisableKey;
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(15);

        private readonly ISystemDisableManager _disableManager;
        private readonly IPropertiesManager _properties;

#if !(RETAIL)
        private Timer _checkMemoryStatusTimer;
#endif
        private bool _disabled;

        public MemoryMonitor()
            : this(
                ServiceManager.GetInstance().GetService<ISystemDisableManager>(),
                ServiceManager.GetInstance().GetService<IPropertiesManager>())
        {
        }

        public MemoryMonitor(
            ISystemDisableManager disableManager,
            IPropertiesManager propertiesManager)
        {
            _disableManager = disableManager ?? throw new ArgumentNullException(nameof(disableManager));
            _properties = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => nameof(MemoryMonitor);

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(MemoryMonitor) };

        /// <inheritdoc />
        public void Initialize()
        {
            _memoryLeftThreshold = (ulong)_properties.GetValue(ApplicationConstants.LowMemoryThreshold, ApplicationConstants.LowMemoryThresholdDefault); //Default of 200MB
#if !(RETAIL)
            _checkMemoryStatusTimer = new Timer(MemoryCheck, null, TimeSpan.Zero, Interval);
#endif

            Logger.Info("Initialized");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

#if !(RETAIL) 
            if (disposing)
            {
                _checkMemoryStatusTimer?.Dispose();
            }
#endif

            _disposed = true;
        }

        /// <summary>
        /// Check memory to see if system is running low on memory.
        /// </summary>
        /// <param name="state">Not used, but required in signature</param>
        /// <remarks>This will lockup the game and request an attendant.</remarks>
        private void MemoryCheck(object state)
        {
            var msex = new MemoryStatusEx();

            //Values are in Bytes, MemoryLoad is a percentage
            NativeMethods.GlobalMemoryStatusEx(msex);

            if(msex.ullAvailPhys <= _memoryLeftThreshold && !_disabled)
            { //If we have exceeded the threshhold, start the lockup process if we haven't already.
                Logger.Error($"Computer Memory is full. Locking up system for reboot. Available Memory: {msex.ullAvailPhys}. Total Memory: {msex.ullTotalPhys}. Threshold: {_memoryLeftThreshold}.");
                _disabled = true;

                _disableManager.Disable(LockupId, SystemDisablePriority.Immediate,
                    ResourceKeys.OutOfMemoryMessage,
                    CultureProviderType.Operator);
            }
        }
    }


    [CLSCompliant(false)]
    internal static class NativeMethods
    {
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MemoryStatusEx lpBuffer);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal class MemoryStatusEx
    {
#pragma warning disable 0649 //Is assigned at runtime
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;
#pragma warning restore 0649


        /// <summary>
        /// Initializes a new instance of the <see cref="T:MEMORYSTATUSEX"/> class.
        /// </summary>
        public MemoryStatusEx()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MemoryStatusEx));
        }
    }
}