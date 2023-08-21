namespace Aristocrat.Monaco.G2S.Data.EventHandler
{
    using Microsoft.EntityFrameworkCore;
    using Common.Storage;
    using Model;

    /// <summary>
    ///     Base interface for supported event repository.
    /// </summary>
    public interface ISupportedEventRepository : IRepository<SupportedEvent>
    {
        /// <summary>
        ///     Gets single supported event by keys like event code and device id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="eventCode">Event code.</param>
        /// <param name="deviceId">Device id.</param>
        /// <returns>Returns supported event or null.</returns>
        SupportedEvent Get(DbContext context, string eventCode, int deviceId);

        /// <summary>
        ///     Deletes single supported event by keys like event code and device id.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="eventCode">Event code.</param>
        /// <param name="deviceId">Device id.</param>
        void Delete(DbContext context, string eventCode, int deviceId);
    }
}