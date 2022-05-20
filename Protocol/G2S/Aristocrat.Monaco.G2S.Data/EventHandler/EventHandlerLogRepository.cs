namespace Aristocrat.Monaco.G2S.Data.EventHandler
{
    using System.Data.Entity;
    using System.Linq;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Event handler log repository implementation.
    /// </summary>
    public class EventHandlerLogRepository : BaseRepository<EventHandlerLog>, IEventHandlerLogRepository
    {
        /// <inheritdoc />
        public EventHandlerLog Get(DbContext context, long eventId, int hostId)
        {
            return context.Set<EventHandlerLog>().FirstOrDefault(
                item => item.HostId == hostId && item.EventId == eventId);
        }

        /// <inheritdoc />
        public void DeleteOldest(DbContext context, long hostId, int count)
        {
            var oldest =
                Get(context, item => item.HostId == hostId).OrderBy(item => item.Id).Take(count);

            DeleteAll(context, oldest);
        }
    }
}