namespace Aristocrat.Monaco.Common
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// This interface is used to expose the functions found in the .net Stopwatch class
    /// </summary>
    public interface IStopwatch
    {
        /// <summary>
        /// 
        /// </summary>
        public void Start();
        /// <summary>
        /// 
        /// </summary>
        public void Stop();
        /// <summary>
        /// 
        /// </summary>
        public void Reset();
        /// <summary>
        /// 
        /// </summary>
        public void Restart();
        /// <summary>
        /// 
        /// </summary>
        public bool IsRunning { get; }
        /// <summary>
        /// 
        /// </summary>
        public TimeSpan Elapsed { get; }
        /// <summary>
        /// 
        /// </summary>
        public long ElapsedMilliseconds { get; }
        /// <summary>
        /// 
        /// </summary>
        public long ElapsedTicks { get; }
    }
}