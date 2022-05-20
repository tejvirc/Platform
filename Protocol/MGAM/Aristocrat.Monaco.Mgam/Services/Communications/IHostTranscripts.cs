namespace Aristocrat.Monaco.Mgam.Services.Communications
{
    using System;
    using Aristocrat.Mgam.Client.Routing;
    using Common;

    /// <summary>
    ///     Provides a record of all the messages sent to and from the server.
    /// </summary>
    public interface IHostTranscripts
    {
        /// <summary>
        ///     Subscribe for messages sent to and from the server.
        /// </summary>
        /// <param name="observer"><see cref="IObserver{T}"/></param>
        /// <returns>Subscription.</returns>
        IDisposable Subscribe(IObserver<RoutedMessage> observer);

        /// <summary>
        ///     Subscribe for transport status changes.
        /// </summary>
        /// <param name="observer"><see cref="IObserver{T}"/></param>
        /// <returns>Subscription.</returns>
        IDisposable Subscribe(IObserver<ConnectionState> observer);

        /// <summary>
        ///     Subscribe for registered instance changes.
        /// </summary>
        /// <param name="observer"><see cref="IObserver{T}"/></param>
        /// <returns>Subscription.</returns>
        IDisposable Subscribe(IObserver<RegisteredInstance> observer);
    }
}
