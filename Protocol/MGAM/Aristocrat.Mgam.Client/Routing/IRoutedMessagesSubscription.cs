namespace Aristocrat.Mgam.Client.Routing
{
    using System;

    /// <summary>
    ///     Publishes messages sent to and from the server.
    /// </summary>
    public interface IRoutedMessagesSubscription
    {
        /// <summary>
        ///     Subscribes to routed messages.
        /// </summary>
        /// <param name="observer"><see cref="IObserver{T}"/></param>
        /// <returns>Subscription.</returns>
        IDisposable Subscribe(IObserver<RoutedMessage> observer);
    }
}
