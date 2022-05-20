namespace Aristocrat.G2S.Emdi.Meters
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Protocol.v21ext1b1;

    /// <summary>
    /// Interface for managing media content subscriptions to meters
    /// </summary>
    public interface IMeterSubscriptions
    {
        /// <summary>
        /// Add subscription for meter for the media content with specified port
        /// </summary>
        /// <param name="port"></param>
        /// <param name="meterName"></param>
        /// <param name="meterType"></param>
        /// <returns></returns>
        Task AddAsync(int port, string meterName, t_meterTypes meterType);

        /// <summary>
        /// Remove subscription for meter for the media content with specified port
        /// </summary>
        /// <param name="port"></param>
        /// <param name="meterName"></param>
        /// <returns></returns>
        Task RemoveAsync(int port, string meterName);

        /// <summary>
        /// Remove all subscriptions for the media content with specified port
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        Task RemoveAllAsync(int port);

        /// <summary>
        /// Gets a list of meter subscriptions meters that the media content (identified by the port) is subscribed to
        /// </summary>
        /// <param name="port"></param>
        /// <returns></returns>
        Task<IList<MeterSubscription>> GetSubscriptionsAsync(int port);

        /// <summary>
        /// Get a list of media content subscribers for specified meter
        /// </summary>
        /// <param name="meterNames"></param>
        /// <returns>A list of subscribers</returns>
        Task<IList<MeterSubscriber>> GetSubscribersAsync(params string[] meterNames);
    }
}