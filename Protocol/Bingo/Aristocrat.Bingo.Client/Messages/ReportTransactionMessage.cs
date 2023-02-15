namespace Aristocrat.Bingo.Client.Messages
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     The message for reporting transactions to the server
    /// </summary>
    [ProtoContract]
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
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public ReportTransactionMessage()
        { }

        /// <summary>
        ///     Gets the machine serial
        /// </summary>
        [ProtoMember(1)]
        public string MachineSerial { get; }

        /// <summary>
        ///     Gets the timestamp for the event
        /// </summary>
        [ProtoMember(2)]
        public DateTime TimeStamp { get; }

        /// <summary>
        ///     Gets the amount for this transaction
        /// </summary>
        [ProtoMember(3)]
        public long Amount { get; }

        /// <summary>
        ///     Gets the game serial or zero that the transaction occured for
        /// </summary>
        [ProtoMember(4)]
        public long GameSerial { get; }

        /// <summary>
        ///     Gets the game title ID for this transaction
        /// </summary>
        [ProtoMember(5)]
        public uint GameTitleId { get; }

        /// <summary>
        ///     Gets the transaction ID
        /// </summary>
        [ProtoMember(6)]
        public long TransactionId { get; }

        /// <summary>
        ///     Gets the paytable Id for this transaction
        /// </summary>
        [ProtoMember(7)]
        public int PaytableId { get; }

        /// <summary>
        ///     Gets the denomination ID for this transaction
        /// </summary>
        [ProtoMember(8)]
        public int DenominationId { get; }

        /// <summary>
        ///     Gets the transaction Type
        /// </summary>
        [ProtoMember(9)]
        public int TransactionType { get; }

        /// <summary>
        ///     Gets the barcode for this transaction
        /// </summary>
        [ProtoMember(10)]
        public string Barcode { get; }
    }
}