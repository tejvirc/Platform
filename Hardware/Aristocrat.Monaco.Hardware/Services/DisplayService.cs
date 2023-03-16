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

    public class DisplayService : IService, IDisplayService
    {
        private readonly ICabinetDetectionService _cabinetDetection;

        private readonly IReadOnlyDictionary<string, int> _frameRates = new Dictionary<string, int>
        {
            { "GT630", 30 }
        };

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
            NVIDIA.Initialize();
            var driverVersion = NVIDIA.DriverVersion.ToString();
            var physicalGpUs = PhysicalGPU.GetPhysicalGPUs();
            var fullName = physicalGpUs[0].FullName;
            var gpuArchitectureName = physicalGpUs[0].ArchitectInformation.ShortName;
            var serial = Encoding.ASCII.GetString(physicalGpUs[0].Board.SerialNumber).Replace("\0", "0");
            var vBios = physicalGpUs[0].Bios.VersionString;
            var totalGpuRam = ((int)physicalGpUs[0].MemoryInformation.DedicatedVideoMemoryInkB / 1024).ToString(); //kb to mB
            var currentTemp = physicalGpUs[0].ThermalInformation.ThermalSensors.ToList()[0].CurrentTemperature;

            return new GpuInfo
            {
                GpuFullName = fullName,
                GpuArchitectureName = gpuArchitectureName,
                SerialNumber = serial,
                BiosVersion = vBios,
                DriverVersion = driverVersion,
                TotalGpuRam = totalGpuRam,
                CurrentGpuTemp = currentTemp
            };
        }
    }
}