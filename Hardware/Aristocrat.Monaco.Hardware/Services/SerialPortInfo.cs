namespace Aristocrat.Monaco.Hardware.Services
{
    /// <summary>
    ///     Serial Port information
    /// </summary>
    public class SerialPortInfo
    {
        public int Address { get; set; }

        public SerialPortType SerialPortType { get; set; }

        public string LogicalPortName { get; set; } = string.Empty;

        public string PhysicalPortName { get; set; } = string.Empty;

        public bool Registered { get; set; }
    }
}