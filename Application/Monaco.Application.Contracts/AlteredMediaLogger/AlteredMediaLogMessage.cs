namespace Aristocrat.Monaco.Application.Contracts.AlteredMediaLogger
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    ///     Definition for AlteredMediaLogMessage
    /// </summary>
    public class AlteredMediaLogMessage : IEquatable<AlteredMediaLogMessage>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AlteredMediaLogMessage"/>
        ///     class to empty, not null.
        /// </summary>
        public AlteredMediaLogMessage()
        {
            MediaType = string.Empty;
            ReasonForChange = string.Empty;
            TimeStamp = DateTime.MinValue;
            Authentication = string.Empty;
            TransactionId = long.MinValue;
        }

        /// <summary>
        ///     Gets or sets the date and time the Media is altered.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets the Media type
        /// </summary>
        [XmlAttribute]
        public string MediaType { get; set; }

        /// <summary>
        ///     Gets or sets the reason for media change
        /// </summary>
        [XmlAttribute]
        public string ReasonForChange { get; set; }

        /// <summary>
        ///     Gets or sets the authentication information for the change
        /// </summary>
        [XmlAttribute]
        public string Authentication { get; set; }

        /// <summary>
        ///     Gets or sets the transactionId.
        /// </summary>
        [XmlAttribute]
        public long TransactionId { get; set; }

        /// <inheritdoc />
        public bool Equals(AlteredMediaLogMessage other)
        {
            if (other == null)
            {
                return false;
            }

            return other.MediaType == MediaType && other.ReasonForChange == ReasonForChange &&
                   other.TimeStamp == TimeStamp && other.TransactionId == TransactionId;
        }
    }
}