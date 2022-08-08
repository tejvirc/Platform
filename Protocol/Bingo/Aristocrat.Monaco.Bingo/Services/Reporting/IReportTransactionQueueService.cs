namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using Common;

    /// <summary>
    ///     Interface for Consumers to use to report bingo server transactions
    /// </summary>
    public interface IReportTransactionQueueService
    {
        /// <summary>
        ///     Adds a bingo server transaction to the queue
        /// </summary>
        /// <param name="transactionType">The transaction type</param>
        /// <param name="amount">The amount of the transaction</param>
        /// <param name="gameSerial">The serial number of the game being played</param>
        /// <param name="gameTitleId">The game title id of the game being played</param>
        /// <param name="paytableId">The paytable id for the game being played</param>
        /// <param name="denominationId">The denomination id being played</param>
        void AddNewTransactionToQueue(
            TransactionType transactionType,
            long amount,
            uint gameTitleId,
            int denominationId,
            long gameSerial = 0,
            int paytableId = 0);
    }
}