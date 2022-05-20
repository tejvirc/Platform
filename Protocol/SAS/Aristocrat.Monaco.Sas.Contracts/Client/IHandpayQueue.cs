namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Aristocrat.Sas.Client.LongPollDataClasses;

    /// <summary>
    ///     The event handler for when a handpay result is read and acknowledged
    /// </summary>
    /// <param name="clientId">The client ID for this event</param>
    /// <param name="transactionId">The transaction ID that was read and acknowledged</param>
    public delegate void HandpayAcknowledged(byte clientId, long transactionId);

    /// <summary>
    ///     A handpay queue used for storing the data needed for a given host when a handpay occurs
    /// </summary>
    public interface IHandpayQueue : IEnumerable<LongPollHandpayDataResponse>
    {
        /// <summary>
        ///     Gets the number of items in the handpay queue
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     The event handler for when a handpay is read and acknowledged
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        event HandpayAcknowledged OnAcknowledged;

        /// <summary>
        ///     Enqueue the data into the Handpay queue
        /// </summary>
        /// <param name="data">The handpay data to enqueue</param>
        /// <returns>The task for enqueue the handpay data</returns>
        Task Enqueue(LongPollHandpayDataResponse data);

        /// <summary>
        ///     Peeks at the next available item in the handpay queue
        /// </summary>
        /// <returns>The next available item in the handpay queue or null if the queue is empty</returns>
        LongPollHandpayDataResponse Peek();

        /// <summary>
        ///     Gets the next available item in the handpay queue and marks it as pending acknowledgement
        /// </summary>
        /// <returns>The next available item in the handpay queue or null if the queue is empty</returns>
        LongPollHandpayDataResponse GetNextHandpayData();

        /// <summary>
        ///     Marks the pending handpay data as acknowledged and clears it out of the queue
        /// </summary>
        /// <returns>The acknowledged task</returns>
        Task HandpayAcknowledged();

        /// <summary>
        ///     Clears any pending handpay items to prevent data loss
        /// </summary>
        void ClearPendingHandpay();
    }
}