namespace Aristocrat.G2S.Emdi.Events
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for managing media content subscriptions to events
    /// </summary>
    public interface IEventSubscriptions
    {
        /// <summary>
        /// Add subscription for event for the media content with specified port
        /// </summary>
        /// <param name="port"></param>
        /// <param name="eventCode"></param>
        /// <returns></returns>
        Task AddAsync(int port, string eventCode);

        /// <summary>
        /// Remove subscription for event for the media content with specified port
        /// </summary>
        /// <param name="port"></param>
        /// <param name="eventCode"></param>
        /// <returns></returns>
        Task RemoveAsync(int port, string eventCode);

        /// <summary>
        /// Remove all subscriptions for the media content with specified port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        Task RemoveAllAsync(int port);

        /// <summary>
        /// Gets a list of events that the media content (identified by the port) is subscribed to
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        Task<IList<EventSubscription>> GetSubscriptionsAsync(int port);

        /// <summary>
        /// Get a list of media content subscribers for specified event
        /// </summary>
        /// <param name="eventCodes"></param>
        /// <returns>A list of subscribers</returns>
        Task<IList<EventSubscriber>> GetSubscribersAsync(params string[] eventCodes);
    }
}