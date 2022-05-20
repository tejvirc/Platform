namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System.Collections.Generic;

    /// <summary>
    ///     A helper class for how to persist and recover queued items
    /// </summary>
    /// <typeparam name="TQueueItem">The type of item stored in the queue</typeparam>
    /// <typeparam name="TQueueId">The type for the id for the queued items</typeparam>
    public interface IAcknowledgedQueueHelper<TQueueItem, out TQueueId>
        where TQueueItem : class
        where TQueueId : struct
    {
        /// <summary>
        ///     Gets the Id field from the message
        /// </summary>
        /// <param name="item">an instance of the message</param>
        /// <returns>The Id field for the message</returns>
        TQueueId GetId(TQueueItem item);

        /// <summary>
        ///     Persists the list of messages
        /// </summary>
        /// <param name="list">The list of messages to persist</param>
        void WritePersistence(List<TQueueItem> list);

        /// <summary>
        ///     Retrieves the list of persisted messages
        /// </summary>
        /// <returns>A list with the persisted messages</returns>
        List<TQueueItem> ReadPersistence();

        /// <summary>
        ///     Creates a tilt when the queue is almost full
        /// </summary>
        void AlmostFullDisable();

        /// <summary>
        ///     Clears the queue is almost full tilt
        /// </summary>
        void AlmostFullClear();
    }
}