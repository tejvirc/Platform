namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using Contracts;
    //using DiskTool;
    using Kernel;
    using Kernel.Contracts.Components;
    using log4net;
    using Properties;

    /// <summary>
    ///     Provides implementation of OS Service. This component handles mapping and handling of physical input events
    ///     from IO services and posting the associated logical events to the system. Also provides and interface for handling
    ///     the device from operator menu.
    /// </summary>
    public class OSService : IOSService, IDisposable
    {
        private const int DiskDriveId = 0; /* Change this if you wish to test Virtual Partitions on a different drive than first drive */

        private const string VersionInfoPath = @"/Tools";
        private const string VersionInfoFile = @"OS_Image_Version.txt";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private bool _disposed;

        private Timer _stats;

        public OSService()
        {
            _stats = new Timer(OnCollectStats, null, TimeSpan.Zero, TimeSpan.FromHours(1));
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
            UpdateDiskInfo();

            OsImageVersion = GetOsImageVersion();

            RegisterComponents();
        }

        public Version OsImageVersion { get; private set; }

        public IList<VirtualPartition> VirtualPartitions { get; private set; } = new List<VirtualPartition>();

        private static Version GetOsImageVersion()
        {
            //try
            //{
            //    var pathMapper = ServiceManager.GetInstance().GetService<IPathMapper>();

            //    var directory = pathMapper.GetDirectory(VersionInfoPath);

            //    var versionInfo = Path.Combine(directory.FullName, VersionInfoFile);

            //    if (File.Exists(versionInfo))
            //    {
            //        var contents = File.ReadAllLines(versionInfo);

            //        if (contents.Length >= 1 && Version.TryParse(contents[0], out var version))
            //        {
            //            return version;
            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Logger.Error("Failed to get OS version", e);
            //}

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

        private void UpdateDiskInfo()
        {
            try
            {
                Update(DiskDriveId); // Always use Disk 0 (Developer disk will always return 0 virtual partitions)
            }
            catch (FileNotFoundException e)
            {
                Logger.Error($"Failed Reading Virtual Partitions: {e.Message}");
            }
        }

        protected virtual void Dispose(bool disposing)
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

        private void Update(int diskId)
        {
            Logger.Warn("OSService.Update not supported in Linux");
            //using (var disk = DriveIO.OpenDisk(diskId)) /* read only */
            //{
            //    if (disk == null)
            //    {
            //        throw new FileNotFoundException($"Could not Open Disk {diskId}");
            //    }

            //    try
            //    {
            //        var mbrData = new byte[512];

            //        disk.Read(mbrData, 512);

            //        var partitionTable = PartitionTable.FromMBR(mbrData);

            //        var endOfvPart = (partitionTable[3].LBAStart + partitionTable[3].NumSectors) * 0x200L;
            //        disk.Seek(endOfvPart, SeekOrigin.Begin);

            //        // clear
            //        VirtualPartitions = new List<VirtualPartition>();
            //        while (true)
            //        {
            //            var vPart = new byte[512];
            //            if (disk.Read(vPart, 512) == 512)
            //            {
            //                if (vPart[0] == 'V' && vPart[1] == 'P' && vPart[2] == 'R' && vPart[3] == 'T')
            //                {
            //                    var vp = new VirtualPartition
            //                    {
            //                        Block = vPart,
            //                        Name = Encoding.Default.GetString(vPart, 4, 64),
            //                        Hash = vPart.Skip(68).Take(20).ToArray(),
            //                        Sig = vPart.Skip(88).Take(40).ToArray(),
            //                        SourcePartition = (int)BitConverter.ToUInt32(vPart, 128),
            //                        SourceOffset = BitConverter.ToInt64(vPart, 132),
            //                        TargetPartition = (int)BitConverter.ToUInt32(vPart, 140),
            //                        Size = BitConverter.ToInt64(vPart, 144),
            //                        SourceFile = Encoding.Default.GetString(vPart, 152, 64),
            //                        State = (PartitionState)BitConverter.ToUInt32(vPart, 216)
            //                    };

            //                    if (vp.State == PartitionState.Active)
            //                    {
            //                        VirtualPartitions.Add(vp);
            //                    }
            //                }
            //                else
            //                {
            //                    break;
            //                }
            //            }
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        throw new FileNotFoundException(e.Message);
            //    }
            //}
        }

        private void RegisterComponents()
        {
            var osHash = VirtualPartitions.GetOperatingSystemHash();
            if (osHash.Length > 0)
            {
                var component = new Component
                {
                    ComponentId = $"ATI_{Environment.OSVersion.Platform}-{Environment.OSVersion.Version}_{OsImageVersion}".Replace(" ", "_"),
                    Type = ComponentType.OS,
                    Description = Resources.OSPackageDescription,
                    FileSystemType = FileSystemType.Stream,
                    Path = HardwareConstants.OperatingSystemPath,
                    Size = osHash.Length
                };

                ServiceManager.GetInstance().GetService<IComponentRegistry>().Register(component);
            }
        }
    }
}