namespace Aristocrat.Bingo.Client.Messages
{
    using System;

    /// <summary>
    ///     The message for reporting transactions to the server
    /// </summary>
    [Serializable]
    public class ReportTransactionMessage : IMessage
    {
        /// <summary>
        ///     Creates an instance of <see cref="ReportTransactionMessage"/>
        /// </summary>
        /// <param name="machineSerial">The machine serial number for this message</param>
        /// <param name="timeStamp">The timestamp for this transaction</param>
        /// <param name="amount">The amount for the transaction</param>
        /// <param name="gameSerial">The game serial for this transaction</param>
        /// <param name="gameTitleId">The game titleId for this transaction</param>
        /// <param name="transactionId">The transactionId</param>
        /// <param name="paytableId">The paytable ID for this transaction</param>
        /// <param name="denominationId">The denomination ID for this transaction</param>
        /// <param name="transactionType">The transaction type</param>
        /// <param name="barcode">The barcode for this transaction</param>
        public ReportTransactionMessage(
            string machineSerial,
            DateTime timeStamp,
            long amount,
            long gameSerial,
            uint gameTitleId,
            long transactionId,
            int paytableId,
            int denominationId,
            int transactionType,
            string barcode)
        {
            MachineSerial = machineSerial;
            TimeStamp = timeStamp;
            Amount = amount;
            GameSerial = gameSerial;
            GameTitleId = gameTitleId;
            TransactionId = transactionId;
            PaytableId = paytableId;
            DenominationId = denominationId;
            TransactionType = transactionType;
            Barcode = barcode;
        }

        /// <summary>
        ///     Gets the machine serial
        /// </summary>
        public string MachineSerial { get; }

        /// <summary>
        ///     Gets the timestamp for the event
        /// </summary>
        public DateTime TimeStamp { get; }

        /// <summary>
        ///     Gets the amount for this transaction
        /// </summary>
        public long Amount { get; }

        /// <summary>
        ///     Gets the game serial or zero that the transaction occured for
        /// </summary>
        public long GameSerial { get; }

        /// <summary>
        ///     Gets the game title ID for this transaction
        /// </summary>
        public uint GameTitleId { get; }

        /// <summary>
        ///     Gets the transaction ID
        /// </summary>
        public long TransactionId { get; }

        /// <summary>
        ///     Gets the paytable Id for this transaction
        /// </summary>
        public int PaytableId { get; }

        /// <summary>
        ///     Gets the denomination ID for this transaction
        /// </summary>
        public int DenominationId { get; }

        /// <summary>
        ///     Gets the transaction Type
        /// </summary>
        public int TransactionType { get; }

        /// <summary>
        ///     Gets the barcode for this transaction
        /// </summary>
        public string Barcode { get; }
    }
}