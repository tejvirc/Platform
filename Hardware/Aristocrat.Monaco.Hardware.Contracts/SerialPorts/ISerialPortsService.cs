namespace Aristocrat.Monaco.Hardware.Contracts.SerialPorts
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     The ISerialPortsService interface provides public access to serial ports.
    /// </summary>
    public interface ISerialPortsService
    {
        /// <summary>
        ///     Get a list of all logical serial port names.
        /// </summary>
        /// <returns>List of all logical serial port names.</returns>
        IEnumerable<string> GetAllLogicalPortNames();

        /// <summary>
        ///     Reserve the use of a port
        /// </summary>
        /// <param name="portName">Which port to reserve</param>
        /// <exception cref="ArgumentOutOfRangeException">if portName is unknown</exception>
        /// <exception cref="ArgumentException">if port is already registered elsewhere</exception>
        void RegisterPort(string portName);

        /// <summary>
        ///     Release use of a serial port
        /// </summary>
        /// <param name="portName">Which port to release</param>
        /// <exception cref="ArgumentOutOfRangeException">if portName is unknown</exception>
        void UnRegisterPort(string portName);

        /// <summary>
        ///     Convert logical port name to physical port name
        /// </summary>
        /// <param name="logicalPortName">Logical port name</param>
        /// <returns>Physical port name</returns>
        string LogicalToPhysicalName(string logicalPortName);

        /// <summary>
        ///     Convert physical port name to logical port name
        /// </summary>
        /// <param name="physicalPortName">Physical port name</param>
        /// <returns>Logical port name</returns>
        string PhysicalToLogicalName(string physicalPortName);
    }
}
