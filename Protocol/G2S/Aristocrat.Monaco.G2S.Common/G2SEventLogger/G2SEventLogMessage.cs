namespace Aristocrat.Monaco.G2S.Common.G2SEventLogger
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    ///     Definition for G2SEventLogMessage
    /// </summary>
    public class G2SEventLogMessage : IEquatable<G2SEventLogMessage>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="G2SEventLogMessage"/>
        ///     class to empty, not null.
        /// </summary>
        public G2SEventLogMessage()
        {
            TimeStamp = DateTime.MinValue;
            EventCode = string.Empty;
            InternalEventType = string.Empty;
            TransactionId = long.MinValue;
        }

        /// <summary>
        ///     Gets or sets the date and time G2S event is sent
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets the event code
        /// </summary>
        [XmlAttribute]
        public string EventCode { get; set; }

        /// <summary>
        ///     Gets or sets the internal event type
        /// </summary>
        [XmlAttribute]
        public string InternalEventType { get; set; }

        /// <summary>
        ///     Gets or sets the transactionId.
        /// </summary>
        [XmlAttribute]
        public long TransactionId { get; set; }

        /// <inheritdoc />
        public bool Equals(G2SEventLogMessage other)
        {
            if (other == null)
            {
                return false;
            }

            return other.EventCode == EventCode && other.InternalEventType == InternalEventType &&
                   other.TimeStamp == TimeStamp && other.TransactionId == TransactionId;
        }
    }
}
