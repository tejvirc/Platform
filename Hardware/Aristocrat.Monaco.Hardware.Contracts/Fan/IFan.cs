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
    }
}