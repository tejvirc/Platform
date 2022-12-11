﻿namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using ProtoBuf;
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    ///     Used for displaying events that happened during a particular game
    /// </summary>
    [ProtoContract]
    public class GameEventLogEntry
    {
        /// <summary>
        ///     A constructor for GameEventLogEntry
        /// </summary>
        /// <param name="entryDate">The date of the entry</param>
        /// <param name="logType">The log type</param>
        /// <param name="logEntry">The log entry</param>
        /// <param name="transactionId">The ID of the transaction</param>
        public GameEventLogEntry(DateTime entryDate, string logType, string logEntry, long transactionId)
        {
            EntryDate = entryDate;
            LogType = logType;
            LogEntry = logEntry;
            TransactionId = transactionId;
        }
        /// <summary>
        /// Parameterless constructor used while deseriliazing 
        /// </summary>
        public GameEventLogEntry()
        {
        }

        /// <summary>
        ///     The date of the entry
        /// </summary>
        [ProtoMember(1)]
        public DateTime EntryDate { get; set; }

        /// <summary>
        ///     The log type
        /// </summary>
        [ProtoMember(2)]
        public string LogType { get; set; }

        /// <summary>
        ///     The log entry
        /// </summary>
        [ProtoMember(3)]
        public string LogEntry { get; set; }

        /// <summary>
        ///     The ID of the transaction
        /// </summary>
        [ProtoMember(4)]
        public long TransactionId { get; set; }
    }
}
