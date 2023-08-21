namespace Aristocrat.Monaco.Gaming.Progressives
{
    using System.Timers;

    /// <summary>
    ///     Provides an interface to get a progressive broadcast timer. This timer is used to monitor
    ///     progressive levels for timeouts.
    /// </summary>
    public interface IProgressiveBroadcastTimer
    {
        /// <summary>
        ///     Gets a linked progressive broadcast timer
        /// </summary>
        Timer Timer { get; }
    }
}