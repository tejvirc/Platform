namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;
    using System.Text;
    using Contracts.Display;
    using Kernel;
    using NvAPIWrapper;
    using NvAPIWrapper.GPU;
    using NvAPIWrapper.Native.Exceptions;

    public class GpuDetailService : IService, IGpuDetailService, IDisposable
    {
        private static PhysicalGPU[] _physicalGpus;
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            NVIDIA.Unload();
            _disposed = true;
        }

        /// <inheritdoc />
        public string ActiveGpuName => GetActiveGetGraphicsCardName();
        /// <inheritdoc />
        public GpuInfo GraphicsCardInfo => GetGraphicsCardDetails();

        /// <inheritdoc />
        public string GpuTemp => RefreshCurrentGpuTemp();

        /// <inheritdoc />
        public bool OnlyIGpuAvailable { get; private set; }

        /// <inheritdoc />
        public string Name => nameof(IGpuDetailService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IGpuDetailService) };

        public void Initialize()
        {
            NVIDIA.Initialize();
            OnlyIGpuAvailable = !TryGetPhysicalGpus();
        }

        public string RefreshCurrentGpuTemp()
        {
            if (OnlyIGpuAvailable)
            {
                return "N/A";
            }

            var sensorInfo = _physicalGpus[0].ThermalInformation.ThermalSensors.ToList()[0].CurrentTemperature;
            var temp = SetToNaIfNoValue(sensorInfo.ToString());
            return temp;
        }

        /// <summary>
        ///     Fetches the physical GPUs from the Nvidia APi through the NVIWrapper package
        /// </summary>
        /// <returns>True if it was able to fetch the physical gpu.</returns>
        private bool TryGetPhysicalGpus()
        {
            try
            {
                _physicalGpus = PhysicalGPU.GetPhysicalGPUs();
                if (_physicalGpus.Length == 0)
                {
                    return false;
                }
            }
            catch (NVIDIAApiException)
            {
                return false;
            }

            return true;
        }

        private string SetToNaIfNoValue(string stringToCheck)
        {
            if (string.IsNullOrEmpty(stringToCheck) || stringToCheck == "0")
            {
                return "N/A";
            }

            return stringToCheck;
        }

        private string SetToNaIfNoValue(object stringToCheck)
        {
            return stringToCheck == null ? "N/A" : stringToCheck.ToString();
        }

        private GpuInfo GetGraphicsCardDetails()
        {
            return OnlyIGpuAvailable ? FetchUsingManagementObjectSearcher() : FetchUsingThirdPartyApi();
        }

        private GpuInfo FetchUsingThirdPartyApi()
        {
            var driverVersion = NVIDIA.DriverVersion.ToString();
            var fullName = SetToNaIfNoValue(_physicalGpus[0].FullName);
            var gpuArchitectureName = SetToNaIfNoValue(_physicalGpus[0].ArchitectInformation.ShortName);
            var serial = _physicalGpus[0].Board.SerialNumber != null
                ? Encoding.ASCII.GetString(_physicalGpus[0].Board.SerialNumber).Replace("\0", "0")
                : "N/A";
            var vBios = SetToNaIfNoValue(_physicalGpus[0]?.Bios.VersionString);
            var totalGpuRamInMb = SetToNaIfNoValue(
                (_physicalGpus[0]?.MemoryInformation.DedicatedVideoMemoryInkB / 1024).ToString());
            var physicalLocation = SetToNaIfNoValue(_physicalGpus[0]?.BusInformation);

            return new GpuInfo
            {
                GpuFullName = fullName,
                GpuArchitectureName = gpuArchitectureName,
                SerialNumber = serial,
                BiosVersion = vBios,
                DriverVersion = driverVersion,
                TotalGpuRam = totalGpuRamInMb,
                PhysicalLocation = physicalLocation
            };
        }

        private GpuInfo FetchUsingManagementObjectSearcher()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
            {
                foreach (var result in searcher.Get())
                {
                    var mo = (ManagementObject)result;

                    var currentBitsPerPixel = mo.Properties["CurrentBitsPerPixel"].Value;
                    var gpuFullName = SetToNaIfNoValue(mo.Properties["Name"].Value);
                    var driverVersion = SetToNaIfNoValue(mo.Properties["DriverVersion"].Value);
                    var totalGpuRam = SetToNaIfNoValue(mo.Properties["AdapterRAM"].Value);

                    if (currentBitsPerPixel != null)
                    {
                        return new GpuInfo
                        {
                            GpuFullName = gpuFullName,
                            GpuArchitectureName = "N/A",
                            SerialNumber = "N/A",
                            BiosVersion = "N/A",
                            DriverVersion = driverVersion,
                            TotalGpuRam = totalGpuRam,
                            PhysicalLocation = "N/A"
                        };
                    }
                }
            }

            return new GpuInfo
            {
                GpuFullName = "N/A",
                GpuArchitectureName = "N/A",
                SerialNumber = "N/A",
                BiosVersion = "N/A",
                DriverVersion = "N/A",
                TotalGpuRam = "N/A",
                PhysicalLocation = "N/A"
            };
        }

        private string GetActiveGetGraphicsCardName()
        {
            return FetchUsingManagementObjectSearcher().GpuFullName;
        }
    }
}