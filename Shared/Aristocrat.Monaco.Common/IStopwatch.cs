namespace Aristocrat.Monaco.Common
{
    using System;

    /// <summary>
    /// This interface is used to expose the functions found in the .net Stopwatch class
    /// </summary>
    public interface IStopwatch
    {
        /// <summary>
        /// Starts, or resumes measuring elapsed time for an interval
        /// </summary>
        public void Start();

        /// <summary>
        /// Stop measuring elapsed time for an interval
        /// </summary>
        public void Stop();

        /// <summary>
        /// Stops the time interval and resets to 0
        /// </summary>
        public void Reset();

        /// <summary>
        /// Stops time interval measurement, resets to 0. Then restarts again.
        /// </summary>
        public void Restart();

        /// <summary>
        /// Gets a value to indicate whether the stopwatch is running or not (Measuring time interval)
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// Get the total elapsed time measured by the current instance
        /// </summary>
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Gets the total elapsed time measured in by the stopwatch for the current instance in Milliseconds.
        /// </summary>
        public long ElapsedMilliseconds { get; }

        /// <summary>
        /// Gets the total elapsed time measured by the stopwatch in Timer Ticks
        /// </summary>
        public long ElapsedTicks { get; }
    }
}