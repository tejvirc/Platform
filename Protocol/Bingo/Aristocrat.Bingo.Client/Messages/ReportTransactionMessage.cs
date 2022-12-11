namespace Aristocrat.Bingo.Client.Messages
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    [ProtoContract]
    public class ReportTransactionMessage : IMessage
    {
        public ReportTransactionMessage(
            string machineSerial,
            DateTime timeStamp,
            long amount,
            long gameSerial,
            uint gameTitleId,
            int transactionId,
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

        [ProtoMember(1)]
        public string MachineSerial { get; }

        [ProtoMember(2)]
        public DateTime TimeStamp { get; }

        [ProtoMember(3)]
        public long Amount { get; }

        [ProtoMember(4)]
        public long GameSerial { get; }

        [ProtoMember(5)]
        public uint GameTitleId { get; }

        [ProtoMember(6)]
        public int TransactionId { get; }

        [ProtoMember(7)]
        public int PaytableId { get; }

        [ProtoMember(8)]
        public int DenominationId { get; }

        [ProtoMember(9)]
        public int TransactionType { get; }

        [ProtoMember(10)]
        public string Barcode { get; }
    }
}