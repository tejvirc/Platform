﻿namespace Aristocrat.Monaco.Gaming.Contracts.Models
{
    using System;

    /// <summary>
    ///     Used for displaying events that happened during a particular game
    /// </summary>
    [Serializable]
    public class GameEventLogEntry : IEquatable<GameEventLogEntry>
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
        ///     The date of the entry
        /// </summary>
        public DateTime EntryDate { get; set; }

        /// <summary>
        ///     The log type
        /// </summary>
        public string LogType { get; set; }

        /// <summary>
        ///     The log entry
        /// </summary>
        public string LogEntry { get; set; }

        /// <summary>
        ///     The ID of the transaction
        /// </summary>
        public long TransactionId { get; set; }
/// <inheritdoc/>

        public bool Equals(GameEventLogEntry obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            GameEventLogEntry other = (GameEventLogEntry)obj;

            return EntryDate == other.EntryDate &&
                   LogType == other.LogType &&
                   LogEntry == other.LogEntry &&
                   TransactionId == other.TransactionId;
        }

        /// <inheritdoc/>

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = EntryDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (LogType != null ? LogType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (LogEntry != null ? LogEntry.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TransactionId.GetHashCode();
                return hashCode;
            }
        }
    }
}
