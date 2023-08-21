namespace Aristocrat.Sas.Client.LongPollDataClasses
{
    using System.Collections.Generic;

    /// <summary>The Transaction Receipt Data Fields for a Set AFT Receipt Data request.</summary>
    /// <remarks>The names in this enum should exactly match the property names in AftReceiptData</remarks>
    public enum TransactionReceiptDataField
    {
        /// <summary>Location</summary>
        Location = 0x00,

        /// <summary>Address 1</summary>
        Address1 = 0x01,

        /// <summary>Address 2</summary>
        Address2 = 0x02,

        /// <summary>In-house line 1</summary>
        InHouse1 = 0x10,

        /// <summary>In-house line 2</summary>
        InHouse2 = 0x11,

        /// <summary>In-house line 3</summary>
        InHouse3 = 0x12,

        /// <summary>In-house line 4</summary>
        InHouse4 = 0x13,

        /// <summary>Debit line 1</summary>
        Debit1 = 0x20,

        /// <summary>Debit line 2</summary>
        Debit2 = 0x21,

        /// <summary>Debit line 3</summary>
        Debit3 = 0x22,

        /// <summary>Debit line 4</summary>
        Debit4 = 0x23,
    }

    /// <summary>Holds the data for a Set Aft Receipt Data request</summary>
    public class SetAftReceiptData : LongPollData
    {
        public Dictionary<TransactionReceiptDataField, string> TransactionReceiptValues =
            new Dictionary<TransactionReceiptDataField, string>();
    }
}