namespace Aristocrat.Mgam.Client.Routing
{
    using System;

    /// <summary>
    ///     Publishes transport status event.
    /// </summary>
    public interface ITransportStatusSubscription
    {
        /// <summary>
        ///     Subscribes to transport events.
        /// </summary>
        /// <param name="observer"><see cref="IObserver{T}"/></param>
        /// <returns>A subscription instance that should be unsubscribed.</returns>
        IDisposable Subscribe(IObserver<TransportStatus> observer);
    }
}
