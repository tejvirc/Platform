namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;
    using System.Text;
    using Contracts.Cabinet;
    using Contracts.Display;
    using Kernel;
    using NvAPIWrapper.GPU;
    using NvAPIWrapper;

    public class DisplayService : IService, IDisplayService, IDisposable
    {
        private readonly ICabinetDetectionService _cabinetDetection;
        private bool _disposed;

        private readonly IReadOnlyDictionary<string, int> _frameRates = new Dictionary<string, int>
        {
            { "GT630", 30 }
        };

        private static PhysicalGPU[] physicalGpUs;

        public DisplayService()
            : this(ServiceManager.GetInstance().GetService<ICabinetDetectionService>())
        {
        }

        public DisplayService(ICabinetDetectionService cabinetDetection)
        {
            _cabinetDetection = cabinetDetection ?? throw new ArgumentNullException(nameof(cabinetDetection));
        }

        public bool IsFaulted => ConnectedCount < ExpectedCount;

        public int ConnectedCount => _cabinetDetection.NumberOfDisplaysConnected;

        public int ExpectedCount { get; private set; }

        public string GraphicsCard => GetGraphicsCard();

        public GpuInfo GraphicsCardInfo => GetGraphicsCardDetails();

        public string GpuTemp => RefreshCurrentGPUTemp();

        public int MaximumFrameRate
        {
            get
            {
                var card = GraphicsCard;
                return _frameRates.Where(x => card.Replace(" ", string.Empty).Contains(x.Key))
                    .Select(x => (int?)x.Value)
                    .FirstOrDefault() ?? -1;
            }
        }

        public string Name => nameof(IDisplayService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IDisplayService) };

        public void Initialize()
        {
            NVIDIA.Initialize();
            ExpectedCount = _cabinetDetection.NumberOfDisplaysConnectedDuringInitialization;
        }


        private static string GetGraphicsCard()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (var result in searcher.Get())
                {
                    var mo = (ManagementObject)result;

                    var currentBitsPerPixel = mo.Properties["CurrentBitsPerPixel"];
                    var description = mo.Properties["Description"];

                    if (currentBitsPerPixel.Value != null)
                    {
                        return description.Value as string;
                    }
                }

                return string.Empty;
            }
        }

        private static GpuInfo GetGraphicsCardDetails()
        {
            physicalGpUs ??= PhysicalGPU.GetPhysicalGPUs();
            var driverVersion = NVIDIA.DriverVersion.ToString();
            var fullName = physicalGpUs[0]?.FullName ?? "N/A";
            var gpuArchitectureName = physicalGpUs[0]?.ArchitectInformation.ShortName ?? "N/A";
            var serial = physicalGpUs[0]?.Board.SerialNumber != null
                ? Encoding.ASCII.GetString(physicalGpUs[0].Board.SerialNumber).Replace("\0", "0")
                : "N/A";
            var vBios = physicalGpUs[0]?.Bios.VersionString ?? "N/A";
            var totalGpuRam =
                ((int)physicalGpUs[0]?.MemoryInformation.DedicatedVideoMemoryInkB / 1024) != 0 ?
                    ((int)physicalGpUs[0]?.MemoryInformation.DedicatedVideoMemoryInkB / 1024).ToString() : 
            "N/A";
            var currentTemperature = physicalGpUs[0].ThermalInformation.ThermalSensors.ToList()[0].CurrentTemperature.ToString();
            var physicalLocation = physicalGpUs[0]?.BusInformation != null
                ? physicalGpUs[0].BusInformation.ToString()
                : " N/A";

            return new GpuInfo
            {
                GpuFullName = fullName,
                GpuArchitectureName = gpuArchitectureName,
                SerialNumber = serial,
                BiosVersion = vBios,
                DriverVersion = driverVersion,
                TotalGpuRam = totalGpuRam,
                CurrentGpuTemp = currentTemperature,
                PhysicalLocation = physicalLocation
            };
        }

        public static string RefreshCurrentGPUTemp()
        {
            physicalGpUs ??= PhysicalGPU.GetPhysicalGPUs();
            var sensorInfo = physicalGpUs[0].ThermalInformation.ThermalSensors.ToList()[0].CurrentTemperature;
            var temp = sensorInfo == 0 ? "N/A" : sensorInfo.ToString();
            return temp;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            NVIDIA.Unload();
            _disposed = true;
        }
    }
}