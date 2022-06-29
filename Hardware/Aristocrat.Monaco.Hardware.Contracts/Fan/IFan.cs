namespace Aristocrat.Monaco.Hardware.Contracts.Fan
{
    /// <summary>
    ///     IBatteryTestService
    /// </summary>
    public interface IFan
    {
        /// <summary>
        ///     Gets fan speed method
        /// </summary>
        /// <returns>fan speed in rpm</returns>
        int GetFanSpeed();

        /// <summary>
        ///     Calculate the fan speed for the given temperature
        /// <parameter name ="temperature"> the current CPU temperature</parameter> 
        /// <returns>Returns fan speed in RPMs</returns>
        /// </summary>
        int CalculateFanSpeed(float temperature);

        /// <summary>
        ///     Calculate the fan speed in PWM for the given RPM
        /// <parameter name ="fanSpeed"> the speed in RPM</parameter> 
        /// </summary>
        /// <returns>Returns fan speed in PWM</returns>
        int CalculatePwm(int fanSpeed);
    }
}