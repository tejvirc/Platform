namespace Aristocrat.Monaco.Sas.Contracts.Client
{
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Defines the interface for interacting with the Aft History Buffer
    /// </summary>
    public interface IAftHistoryBuffer
    {
        /// <summary>
        ///     Gets the current buffer index
        /// </summary>
        byte CurrentBufferIndex { get; }

        /// <summary>
        ///     Add a new transaction to the history buffer
        /// </summary>
        /// <param name="response">The value for the new entry</param>
        /// <returns>The TransactionIndex the entry was saved at</returns>
        byte AddEntry(AftResponseData response);

        /// <summary>
        ///     Add a new transaction to the history buffer
        /// </summary>
        /// <param name="response">The value for the new entry</param>
        /// <param name="work"></param>
        /// <returns>The TransactionIndex the entry was saved at</returns>
        byte AddEntry(AftResponseData response, IUnitOfWork work);

        /// <summary>
        ///     Gets an entry from the history buffer.
        /// </summary>
        /// <param name="entryIndex">entry index.</param>
        /// <returns>The history entry. The entry will have transfer and receipt status
        /// of 0xFF if there isn't any history</returns>
        AftResponseData GetHistoryEntry(byte entryIndex);
    }
}