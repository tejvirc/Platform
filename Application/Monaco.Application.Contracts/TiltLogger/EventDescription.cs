namespace Aristocrat.Monaco.Application.Contracts.TiltLogger
{
    using System;
    using System.Linq;
    using Localization;

    /// <summary>
    ///     Definition of the EventDescription class.
    /// </summary>
    public partial class EventDescription : IEquatable<EventDescription>
    {
        private string _displayText;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventDescription" /> class.  To empty, not null.
        /// </summary>
        public EventDescription()
            : this(string.Empty, string.Empty, string.Empty, long.MinValue, Guid.NewGuid(), DateTime.MinValue)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventDescription" /> class.
        /// </summary>
        /// <param name="name">The parameter is the name.</param>
        /// <param name="level">The parameter is the level.</param>
        /// <param name="type">The parameter is the type.</param>
        /// <param name="transactionId">The parameter is the transactionId.</param>
        /// <param name="timestamp">The parameter is the timestamp.</param>
        /// <param name="additionalInfos">The parameter is the additional info as name value pairs array.</param>
        public EventDescription(string name, string level, string type, long transactionId, DateTime timestamp, (string, string)[] additionalInfos = null) : this(name, level, type, transactionId, Guid.NewGuid(), timestamp, additionalInfos)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EventDescription" /> class.
        /// </summary>
        /// <param name="name">The parameter is the name.</param>
        /// <param name="level">The parameter is the level.</param>
        /// <param name="type">The parameter is the type.</param>
        /// <param name="transactionId">The parameter is the transactionId.</param>
        /// <param name="value">The parameter is the Guid.</param>
        /// <param name="timestamp">The parameter is the timestamp.</param>
        /// <param name="additionalInfos">The parameter is the additional info as name value pairs array.</param>
        public EventDescription(string name, string level, string type, long transactionId, Guid value, DateTime timestamp, (string, string)[] additionalInfos = null)
        {
            Name = name;
            Level = level;
            Type = type;
            TransactionId = transactionId;
            Guid = value;
            Timestamp = timestamp;
            AdditionalInfos = additionalInfos;
        }

        /// <summary>
        ///     Gets or sets the transaction Id.
        /// </summary>
        public long TransactionId { get; set; }

        /// <summary>
        ///     Gets or sets the Guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        ///     Gets or sets the timestamp.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        ///     Gets or sets the Log Sequence.
        /// </summary>
        public long LogSequence { get; set; } = -1;

        /// <summary>
        ///     Gets or sets the additional info as name value pair array.
        /// </summary>
        public (string Name, string Value)[] AdditionalInfos { get; set; }

        /// <summary>
        ///     Gets the display text for this event
        /// </summary>
        public string TypeDisplayText => _displayText ?? (_displayText = Localizer.For(CultureFor.Operator).GetString(Type) ?? Type);

        /// <inheritdoc />
        public bool Equals(EventDescription other)
        {
            if (other == null)
            {
                return false;
            }

            return other.Name == Name && other.Level == Level && other.Type == Type && other.TransactionId == TransactionId && other.Timestamp == Timestamp && other.GetAdditionalInfoString() == GetAdditionalInfoString() && other.TypeDisplayText == TypeDisplayText;
        }

        /// <summary>
        ///     Gets the additional info string joined by ' -- '.
        /// </summary>
        public string GetAdditionalInfoString() => AdditionalInfos != null && AdditionalInfos.Any() ? string.Join(" -- ", from e in AdditionalInfos select e.Value) : default;
    }
}