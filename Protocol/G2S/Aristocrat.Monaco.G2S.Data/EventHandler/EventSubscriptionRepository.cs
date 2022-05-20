namespace Aristocrat.Monaco.G2S.Data.EventHandler
{
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Event subscription repository implementation.
    /// </summary>
    public class EventSubscriptionRepository : BaseRepository<EventSubscription>, IEventSubscriptionRepository
    {
        /// <inheritdoc />
        public EventSubscription Get(DbContext context, string eventCode, int hostId, int deviceId)
        {
            return context.Set<EventSubscription>().FirstOrDefault(
                item => item.DeviceId == deviceId && item.EventCode == eventCode && item.HostId == hostId);
        }

        /// <inheritdoc />
        public EventSubscription Get(
            DbContext context,
            string eventCode,
            int hostId,
            int deviceId,
            EventSubscriptionType type)
        {
            return context.Set<EventSubscription>().FirstOrDefault(
                item =>
                    item.DeviceId == deviceId && item.EventCode == eventCode && item.SubType == type &&
                    item.HostId == hostId);
        }
    }
}