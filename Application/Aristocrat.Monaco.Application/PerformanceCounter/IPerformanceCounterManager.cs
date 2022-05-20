namespace Aristocrat.Monaco.Application.PerformanceCounter
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     This defines the methods that are used to expose the performance counters
    ///     that are captured for diagnostic purpose
    /// </summary>
    public interface IPerformanceCounterManager
    {
        /// <summary>
        ///     Gets the latest performance counters, that the resource monitor asks
        /// </summary>
        PerformanceCounters CurrentPerformanceCounter { get; }

        /// <summary>
        ///     Gets the List of the performance counters for a duration of max last 30 days.
        /// </summary>
        Task<IList<PerformanceCounters>> CountersForParticularDuration(
            DateTime startDate,
            DateTime endDate,
            CancellationToken token);
    }
}