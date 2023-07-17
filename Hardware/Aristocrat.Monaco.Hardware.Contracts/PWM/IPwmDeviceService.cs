namespace Aristocrat.Monaco.Hardware.Contracts.PWM
{
    using SharedDevice;


    /// <summary>
    ///     The pwm device used for communication with PWM devices.
    /// </summary>
    public interface IPwmDeviceService : IDeviceService
    {

        /// <summary>Return the device name.</summary>
        string DeviceName { get; set; }

    }
}
