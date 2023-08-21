namespace Aristocrat.Monaco.Hardware.Contracts.SharedDevice
{
    /// <summary>
    ///     Definition of the IDevice interface.
    /// </summary>
    public interface IDevice : IDeviceConfiguration, ISerialPortController
    {
        /// <summary>
        ///     Get/set the mode
        /// </summary>
        string Mode { get; set; }
    }
}