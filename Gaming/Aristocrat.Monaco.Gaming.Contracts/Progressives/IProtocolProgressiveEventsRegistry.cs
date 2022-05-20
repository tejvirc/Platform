namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using Kernel;

    /// <summary>
    /// An interface that protocols should use to subscribe, unsubscribe and publish progressive events.
    /// </summary>
    public interface IProtocolProgressiveEventsRegistry : IService
    {
        /// <summary>
        /// This method subscribes the event of type T from the event bus with the handler passed only if the
        /// protocol is configured to handle progressives.
        /// </summary>
        /// <param name="protocolName">name of the protocol subscribing the progressive events</param>
        /// <param name="handler">event handler that handles progressive event</param>
        /// <typeparam name="T">the progressive event type to be subscribed for</typeparam>
        void SubscribeProgressiveEvent<T>(string protocolName, IProtocolProgressiveEventHandler handler) where T : IEvent;

        /// <summary>
        /// This method unsubscribes the event of type T from the event bus with the handler passed.
        /// Whichever protocol called the SubscribeProgressiveEvent() method, must call this
        /// method to unsubscribe the events when the protocol is shutting down.
        /// </summary>
        /// <param name="protocolName">name of the protocol unsubscribing the progressive events</param>
        /// <param name="handler">event handler that subscribed via SubscribeProgressiveEvents method earlier</param>
        /// <typeparam name="T">the progressive event type to be unsubscribed for</typeparam>
        void UnSubscribeProgressiveEvent<T>(string protocolName, IProtocolProgressiveEventHandler handler) where T : IEvent;

        /// <summary>
        /// This method used to publish a progressive events from the protocol layer.
        /// </summary>
        /// <param name="protocolName">The name of the protocol publishing the progressive events</param>
        /// <param name="event">the progressive event to be published</param>
        void PublishProgressiveEvent(string protocolName, IEvent @event);
    }
}
