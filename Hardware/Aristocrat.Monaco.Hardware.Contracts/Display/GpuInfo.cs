namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    /// <summary>
    ///     Graphics card info
    /// </summary>
    public class GpuInfo

    {
        /// <summary>
        ///     The graphic processors full name
        /// </summary>
        public string GpuFullName { get; set; }

        /// <summary>
        ///     It's architecture name
        /// </summary>
        public string GpuArchitectureName { get; set; }

        /// <summary>
        ///     It's serial number
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        ///     It's BIOS version
        /// </summary>
        public string BiosVersion { get; set; }

        /// <summary>
        ///     It's driver version
        /// </summary>
        public string DriverVersion { get; set; }

        /// <summary>
        ///     It's RAM in GigaBytes
        /// </summary>
        public string GpuRam { get; set; }

        /// <summary>
        ///     It's physical location on the motherboard (which PCI bus its connected to)
        /// </summary>
        public string PhysicalLocation { get; set; }
    }
}