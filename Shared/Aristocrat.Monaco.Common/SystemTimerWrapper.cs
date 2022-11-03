namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Timers;

    /// <summary>
    ///     SystemTimerWrapper is a general purpose timer. It will need to be disposed of properly.
    /// </summary>
    public class SystemTimerWrapper : Timer, ISystemTimerWrapper
    {
        /// <summary>
        /// 
        /// </summary>
        public SystemTimerWrapper() : base()
        {
                
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeInterval"></param>
        public SystemTimerWrapper(long timeInterval) : base(timeInterval)
        {

        }
    }
}