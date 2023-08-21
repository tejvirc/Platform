namespace Aristocrat.Monaco.Hhr.Client.Communication
{
    using System;
    using Messages;

    /// <summary>
    /// Interface that provides progressive updates received on the UDP
    /// </summary>
    public interface IProgressiveBroadcastService
    {
        /// <summary>
        /// Contains the recent information received from the udp
        /// </summary>
        IObservable<GameProgressiveUpdate> ProgressiveUpdates { get; }
    }
}