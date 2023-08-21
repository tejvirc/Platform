namespace Aristocrat.Monaco.Hardware.Contracts.Battery
{
    /// <summary>
    ///     IBatteryTestService
    /// </summary>
    public interface IBattery
    {
        /// <summary>
        ///     TestBatteries method
        /// </summary>
        /// <returns>The results of the battery test</returns>
        (bool Battery1Result, bool Battery2Result) Test();
    }
}