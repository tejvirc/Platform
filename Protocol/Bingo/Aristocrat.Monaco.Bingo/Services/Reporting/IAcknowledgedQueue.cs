namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     Defines a thread safe queue that must be acknowledged before
    ///     an item is removed from the queue.
    /// </summary>
    /// <typeparam name="TQueueItem">The type of item stored in the queue</typeparam>
    /// <typeparam name="TQueueId">The type for the id for the queued items</typeparam>
    public interface IAcknowledgedQueue<TQueueItem, in TQueueId>
        where TQueueItem : class
        where TQueueId : struct
    {
        /// <summary>
        ///     Gets the next item in the queue but doesn't remove the item.
        ///     This will block if there aren't any items in the queue.
        /// </summary>
        /// <param name="token">a cancellation token</param>
        /// <returns>The next item or null if the queue is empty</returns>
        Task<TQueueItem> GetNextItem(CancellationToken token);

        /// <summary>
        ///     Adds a new item to the queue. If the queue is full the item
        ///     isn't added.
        /// </summary>
        /// <param name="item">The item to be added</param>
        void Enqueue(TQueueItem item);

        /// <summary>
        ///     Acknowledges the server has received the item and
        ///     it can be removed from the queue
        /// </summary>
        /// <param name="id">
        /// The id of the item that was acknowledged.
        /// This must match the id of the item on the top of
        /// the queue or it will be ignored.
        /// </param>
        void Acknowledge(TQueueId id);

        /// <summary>
        ///     Indicates whether the queue is empty or not.
        /// </summary>
        /// <returns>True if the queue is empty, false otherwise</returns>
        bool IsEmpty();

        /// <summary>
        ///     A count of how many items are in the queue
        /// </summary>
        /// <returns>The count of items in the queue</returns>
        int Count();
    }
}