namespace Aristocrat.Bingo.Client.Messages
{
    using System;

    [Serializable]
    public class ReportTransactionMessage : IMessage
    {
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

        public string MachineSerial { get; }

        public DateTime TimeStamp { get; }

        public long Amount { get; }

        public long GameSerial { get; }

        public uint GameTitleId { get; }

        public long TransactionId { get; }

        public int PaytableId { get; }

        public int DenominationId { get; }

        public int TransactionType { get; }

        public string Barcode { get; }
    }
}