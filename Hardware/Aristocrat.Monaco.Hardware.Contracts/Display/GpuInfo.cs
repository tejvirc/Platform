namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    /// <summary>
    /// 
    /// </summary>
    public class GpuInfo

    {
        /// <summary>
        /// 
        /// </summary>
        public string GpuFullName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GpuArchitectureName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string BiosVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string DriverVersion { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string TotalGpuRam { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string PhysicalLocation { get; set; }
    }
}
