namespace Aristocrat.Monaco.G2S.Data.EventHandler
{
    using System.Data.Entity;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Base interface for event subscription repository.
    /// </summary>
    public interface IEventSubscriptionRepository : IRepository<EventSubscription>
    {
        /// <summary>
        ///     Gets single event subscription by keys like event code and device id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="eventCode">Event code.</param>
        /// <param name="hostId">Host Id.</param>
        /// <param name="deviceId">Device id.</param>
        /// <returns>Returns event subscription or null.</returns>
        EventSubscription Get(DbContext context, string eventCode, int hostId, int deviceId);

        /// <summary>
        ///     Gets single event subscription by keys like event code, device id and subscription type.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="eventCode">Event code.</param>
        /// <param name="hostId">Host Id.</param>
        /// <param name="deviceId">Device Id.</param>
        /// <param name="type">Subscription type.</param>
        /// <returns>Returns event subscription or null.</returns>
        EventSubscription Get(
            DbContext context,
            string eventCode,
            int hostId,
            int deviceId,
            EventSubscriptionType type);
    }
}