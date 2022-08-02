namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    /// <summary>
    ///     An interface that behaves like a PerformanceCounter but may include extra calculations
    ///     or functionality involved in generating a value.
    /// </summary>
    public interface IPerformanceCounterWrapper
    {
        /// <summary>
        ///     Get the next value for the performance counter which we want to monitor.
        /// </summary>
        /// <returns>A performance counter value</returns>
        float NextValue();
    }
}