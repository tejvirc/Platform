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
        ///     Gets fan speed method
        /// </summary>
        /// <returns>fan speed in rpm</returns>
        int CalculateFanspeed(int Tempreture);

        /// <summary>
        ///     Gets fan speed method
        /// </summary>
        /// <returns>fan speed in rpm</returns>
        int CalculatePWN(int Fanspeed);
    }
}