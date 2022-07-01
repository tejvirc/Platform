namespace Aristocrat.Monaco.Hardware.Contracts.Fan
{
    using System.Reactive.Subjects;

    /// <summary>
    ///     IBatteryTestService
    /// </summary>
    public interface IFan
    {
        /// <summary>
        /// 
        /// </summary>
        Subject<CpuMetricsInfo> CpuMetrics { get; }

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

    /// <summary>
    /// 
    /// </summary>
    public class CpuMetricsInfo
    {
        /// <summary>
        /// 
        /// </summary>
        public int FanPwm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public float CpuTemperature { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int FanSpeed { get; set; }
    }
}