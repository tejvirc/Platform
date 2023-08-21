namespace Aristocrat.Monaco.Bingo.Services.Reporting
{
    using Common;

    /// <summary>
    ///     Extension methods for <see cref="IReportTransactionQueueService"/>
    /// </summary>
    public static class ReportTransactionQueueServiceExtensions
    {
        private const long DefaultGameSerial = 0;
        private const int DefaultPaytable = 0;
        private const uint DefaultGameTitleId = 0;
        private const int DefaultDenominationId = 0;

        private static readonly string DefaultBarcode = string.Empty;

        /// <summary>
        ///     Adds a bingo server transaction to the queue
        /// </summary>
        /// <param name="service">The <see cref="IReportTransactionQueueService"/> to call</param>
        /// <param name="transactionType">The transaction type</param>
        /// <param name="amount">The amount of the transaction</param>
        /// <param name="gameSerial">The serial number of the game being played</param>
        /// <param name="gameTitleId">The game title id of the game being played</param>
        /// <param name="paytableId">The paytable id for the game being played</param>
        /// <param name="denominationId">The denomination id being played</param>
        public static void AddNewTransactionToQueue(
            this IReportTransactionQueueService service,
            TransactionType transactionType,
            long amount,
            uint gameTitleId,
            int denominationId,
            long gameSerial,
            int paytableId)
        {
            service.AddNewTransactionToQueue(
                transactionType,
                amount,
                gameTitleId,
                denominationId,
                gameSerial,
                paytableId,
                DefaultBarcode);
        }

        /// <summary>
        ///     Adds a bingo server transaction to the queue
        /// </summary>
        /// <param name="service">The <see cref="IReportTransactionQueueService"/> to call</param>
        /// <param name="transactionType">The transaction type</param>
        /// <param name="amount">The amount of the transaction</param>
        /// <param name="barcode">The barcode to for this transaction</param>
        /// <param name="gameTitleId">The game title id of the game being played</param>
        /// <param name="denominationId">The denomination id being played</param>
        public static void AddNewTransactionToQueue(
            this IReportTransactionQueueService service,
            TransactionType transactionType,
            long amount,
            uint gameTitleId,
            int denominationId,
            string barcode)
        {
            service.AddNewTransactionToQueue(
                transactionType,
                amount,
                gameTitleId,
                denominationId,
                DefaultGameSerial,
                DefaultPaytable,
                barcode);
        }

        /// <summary>
        ///     Adds a bingo server transaction to the queue
        /// </summary>
        /// <param name="service">The <see cref="IReportTransactionQueueService"/> to call</param>
        /// <param name="transactionType">The transaction type</param>
        /// <param name="amount">The amount of the transaction</param>
        /// <param name="barcode">The barcode to for this transaction</param>
        public static void AddNewTransactionToQueue(
            this IReportTransactionQueueService service,
            TransactionType transactionType,
            long amount,
            string barcode)
        {
            service.AddNewTransactionToQueue(
                transactionType,
                amount,
                DefaultGameTitleId,
                DefaultDenominationId,
                DefaultGameSerial,
                DefaultPaytable,
                barcode);
        }

        /// <summary>
        ///     Adds a bingo server transaction to the queue
        /// </summary>
        /// <param name="service">The <see cref="IReportTransactionQueueService"/> to call</param>
        /// <param name="transactionType">The transaction type</param>
        /// <param name="amount">The amount of the transaction</param>
        /// <param name="gameTitleId">The game title id of the game being played</param>
        /// <param name="denominationId">The denomination id being played</param>
        public static void AddNewTransactionToQueue(
            this IReportTransactionQueueService service,
            TransactionType transactionType,
            long amount,
            uint gameTitleId,
            int denominationId)
        {
            service.AddNewTransactionToQueue(
                transactionType,
                amount,
                gameTitleId,
                denominationId,
                DefaultGameSerial,
                DefaultPaytable,
                DefaultBarcode);
        }

        /// <summary>
        ///     Adds a bingo server transaction to the queue
        /// </summary>
        /// <param name="service">The <see cref="IReportTransactionQueueService"/> to call</param>
        /// <param name="transactionType">The transaction type</param>
        /// <param name="amount">The amount of the transaction</param>
        public static void AddNewTransactionToQueue(
            this IReportTransactionQueueService service,
            TransactionType transactionType,
            long amount)
        {
            service.AddNewTransactionToQueue(
                transactionType,
                amount,
                DefaultGameTitleId,
                DefaultDenominationId,
                DefaultGameSerial,
                DefaultPaytable,
                DefaultBarcode);
        }
    }
}