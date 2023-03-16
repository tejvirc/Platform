namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
#pragma warning disable CS1591
    public class GpuInfo

    {
        public string GpuFullName { get; set; }

        public string GpuArchitectureName { get; set; }

        public string SerialNumber { get; set; }

        public string BiosVersion { get; set; }

        public string DriverVersion { get; set; }

        public string TotalGpuRam { get; set; }

        public int CurrentGpuTemp { get; set; }
    }
}
#pragma warning restore CS1591