namespace Aristocrat.Monaco.Hardware.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management;
    using System.Text;
    using Contracts.Display;
    using Kernel;
    using Localization.Properties;
    using NvAPIWrapper;
    using NvAPIWrapper.GPU;
    using NvAPIWrapper.Native.Exceptions;

    public class GpuDetailService : IService, IGpuDetailService, IDisposable
    {
        private static PhysicalGPU[] _physicalGpus;
        private bool _disposed;

        /// <inheritdoc />
        public string ActiveGpuName => GetActiveGetGraphicsCardName();

        /// <inheritdoc />
        public GpuInfo GraphicsCardInfo => GetGraphicsCardDetails();

        /// <inheritdoc />
        public string GpuTemp => RefreshCurrentGpuTemp();

        /// <inheritdoc />
        public bool OnlyIGpuAvailable { get; private set; }

        public void Initialize()
        {
            NVIDIA.Initialize();
            OnlyIGpuAvailable = !TryGetPhysicalGpus();
        }

        /// <inheritdoc />
        public string Name => nameof(IGpuDetailService);

        public ICollection<Type> ServiceTypes => new[] { typeof(IGpuDetailService) };

        public string RefreshCurrentGpuTemp()
        {
            if (OnlyIGpuAvailable)
            {
                return ResourceKeys.NotAvailable;
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
                return ResourceKeys.NotAvailable;
            }

            return stringToCheck;
        }

        private string SetToNaIfNoValue(object stringToCheck)
        {
            return stringToCheck == null ? ResourceKeys.NotAvailable : stringToCheck.ToString();
        }

        private GpuInfo GetGraphicsCardDetails()
        {
            return OnlyIGpuAvailable ? FetchUsingManagementObjectSearcher()[0] : FetchUsingThirdPartyApi();
        }

        private GpuInfo FetchUsingThirdPartyApi()
        {
            var driverVersion = NVIDIA.DriverVersion.ToString();
            var fullName = SetToNaIfNoValue(_physicalGpus[0].FullName);
            var gpuArchitectureName = SetToNaIfNoValue(_physicalGpus[0].ArchitectInformation.ShortName);
            var serial = _physicalGpus[0].Board.SerialNumber != null
                ? Encoding.ASCII.GetString(_physicalGpus[0].Board.SerialNumber).Replace("\0", "0")
                : ResourceKeys.NotAvailable;
            var vBios = SetToNaIfNoValue(_physicalGpus[0]?.Bios.VersionString);
            var GpuRamInGB = SetToNaIfNoValue(
                (_physicalGpus[0]?.MemoryInformation.DedicatedVideoMemoryInkB / 1024).ToString());
            var physicalLocation = SetToNaIfNoValue(_physicalGpus[0]?.BusInformation);

            return new GpuInfo
            {
                GpuFullName = fullName,
                GpuArchitectureName = gpuArchitectureName,
                SerialNumber = serial,
                BiosVersion = vBios,
                DriverVersion = driverVersion,
                GpuRam = GpuRamInGB,
                PhysicalLocation = physicalLocation
            };
        }

        private List<GpuInfo> FetchUsingManagementObjectSearcher()
        {
            var foundGpus = new List<GpuInfo>();
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
                        foundGpus.Add(
                            new GpuInfo
                            {
                                GpuFullName = gpuFullName,
                                GpuArchitectureName = ResourceKeys.NotAvailable,
                                SerialNumber = ResourceKeys.NotAvailable,
                                BiosVersion = ResourceKeys.NotAvailable,
                                DriverVersion = driverVersion,
                                GpuRam = totalGpuRam,
                                PhysicalLocation = ResourceKeys.NotAvailable
                            });
                    }
                }
            }

            return foundGpus;
        }

        private string GetActiveGetGraphicsCardName()
        {
            return FetchUsingManagementObjectSearcher()[0].GpuFullName;
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