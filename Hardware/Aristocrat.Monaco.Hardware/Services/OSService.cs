namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Common;
    using Contracts;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using NativeOS.Services.OS;
    using Properties;

    /// <summary>
    ///     Provides implementation of OS Service. This component handles mapping and handling of physical input events
    ///     from IO services and posting the associated logical events to the system. Also provides and interface for handling
    ///     the device from operator menu.
    /// </summary>
    public class OSService : IOSService, IDisposable
    {
        private readonly IComponentRegistry _componentRegistry;
        private readonly IPathMapper _pathMapper;
        private readonly IVirtualPartition _virtualPartition;
        private const int DiskDriveId = 0; /* Change this if you wish to test Virtual Partitions on a different drive than first drive */

        private const string VersionInfoPath = @"/Tools";
        private const string VersionInfoFile = @"OS_Image_Version.txt";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        private bool _disposed;

        private Timer _stats;

        public OSService(IComponentRegistry componentRegistry, IPathMapper pathMapper, IVirtualPartition virtualPartition)
        {
            _componentRegistry = componentRegistry ?? throw new ArgumentNullException(nameof(componentRegistry));
            _pathMapper = pathMapper ?? throw new ArgumentNullException(nameof(pathMapper));
            _virtualPartition = virtualPartition ?? throw new ArgumentNullException(nameof(virtualPartition));
            _stats = new Timer(OnCollectStats, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        public OSService()
            : this(
                ServiceManager.GetInstance().GetService<IComponentRegistry>(),
                ServiceManager.GetInstance().GetService<IPathMapper>(),
                VirtualPartitionProviderFactory.CreateVirtualPartitionProvider())
        {
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string Name => nameof(OSService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IOSService) };

        public void Initialize()
        {
            _virtualPartition.InitializeAsync(CancellationToken.None).WaitForCompletion();
            OsImageVersion = GetOsImageVersion();
            RegisterComponents();
        }

        public Version OsImageVersion { get; private set; }

        public IReadOnlyCollection<VirtualPartition> VirtualPartitions => _virtualPartition.VirtualPartitions;

        public IEnumerable<byte> GetOperatingSystemHash() => _virtualPartition.GetOperatingSystemHash();

        private Version GetOsImageVersion()
        {
            try
            {
                var directory = _pathMapper.GetDirectory(VersionInfoPath);

                var versionInfo = Path.Combine(directory.FullName, VersionInfoFile);

                if (File.Exists(versionInfo))
                {
                    var contents = File.ReadAllLines(versionInfo);

                    if (contents.Length >= 1 && Version.TryParse(contents[0], out var version))
                    {
                        return version;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to get OS version", e);
            }

            return new Version();
        }

        private static void OnCollectStats(object state)
        {
            var process = Process.GetCurrentProcess();

            Logger.Info(
                $@"Current Stats
    Total processor time: {process.TotalProcessorTime}
    Thread count: {process.Threads.Count}
    Handle count: {process.HandleCount}
    Working set: {process.WorkingSet64}
    Handle count: {process.HandleCount}
    Working set: {process.WorkingSet64}
    Peak working set: {process.PeakWorkingSet64}
    Private bytes: {process.PrivateMemorySize64}
    GC total: {GC.GetTotalMemory(false)}");
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _stats.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
                _stats.Dispose();
            }

            _stats = null;

            _disposed = true;
        }

        private void RegisterComponents()
        {
            var osHash = GetOperatingSystemHash().ToArray();
            if (osHash.Length <= 0)
            {
                return;
            }

            var component = new Component
            {
                ComponentId = $"ATI_{Environment.OSVersion.Platform}-{Environment.OSVersion.Version}_{OsImageVersion}".Replace(" ", "_"),
                Type = ComponentType.OS,
                Description = Resources.OSPackageDescription,
                FileSystemType = FileSystemType.Stream,
                Path = HardwareConstants.OperatingSystemPath,
                Size = osHash.Length
            };

            _componentRegistry.Register(component);
        }
    }
}