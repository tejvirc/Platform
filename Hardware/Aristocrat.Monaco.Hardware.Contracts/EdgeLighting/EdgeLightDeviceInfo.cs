namespace Aristocrat.Monaco.Hardware.Contracts.EdgeLighting
{
    /// <summary>
    ///     Defines attributes of a edge light device.
    /// </summary>
    public struct EdgeLightDeviceInfo
    {
        /// <summary>
        ///     Manufacturer of the device.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        ///     Serial number of the device.
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        ///     Product of the device.
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        ///     Version of the device.
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        ///     Edge light device type.
        /// </summary>
        public ElDeviceType DeviceType { get; set; }
    }
}