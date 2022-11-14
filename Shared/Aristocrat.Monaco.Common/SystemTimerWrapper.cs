namespace Aristocrat.Monaco.Common
{
    using System.Timers;

    /// <summary>
    ///     SystemTimerWrapper is a general purpose timer. Found in the .Net System.Timer
    /// </summary>
    public class SystemTimerWrapper : Timer, ISystemTimerWrapper
    {
        /// <summary>
        /// Parameterless constructor for .Net System.Timer
        /// </summary>
        public SystemTimerWrapper()
        {
        }

        /// <summary>
        /// .Net System.Timer constructor with Time Interval between elapsed parameter.
        /// </summary>
        /// <param name="timeInterval">Time between elapsed</param>
        public SystemTimerWrapper(long timeInterval) : base(timeInterval)
        {
        }
    }
}