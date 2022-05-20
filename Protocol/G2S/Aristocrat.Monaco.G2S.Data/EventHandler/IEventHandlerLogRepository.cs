namespace Aristocrat.Monaco.G2S.Data.EventHandler
{
    using System.Data.Entity;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Base interface for event handler log repository.
    /// </summary>
    public interface IEventHandlerLogRepository : IRepository<EventHandlerLog>
    {
        /// <summary>
        ///     Gets single event handler log report data by keys like event id and device id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="eventId">Event Id.</param>
        /// <param name="hostId">Host id.</param>
        /// <returns>Returns event handler log or null.</returns>
        EventHandlerLog Get(DbContext context, long eventId, int hostId);

        /// <summary>
        ///     Deletes oldest event from persistence.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="hostId">Host Id</param>
        /// <param name="count">The total number of records to remove</param>
        void DeleteOldest(DbContext context, long hostId, int count);
    }
}