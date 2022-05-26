namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Definition of PresentationOverrideData
    /// </summary>
    public class PresentationOverrideData
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PresentationOverrideData"/> class. The data is composed of a message and a type
        /// </summary>
        /// <param name="message">The message describing the platform overlay data</param>
        /// <param name="type">The type of presentation to be overridden</param>
        public PresentationOverrideData(string message, PresentationOverrideTypes type)
        {
            Message = message;
            Type = type;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PresentationOverrideData"/> class. The data is composed of a message and a type
        /// </summary>
        /// <param name="message">The message describing the platform overlay data</param>
        /// <param name="formattedAmount">The formatted amount for this transaction</param>
        /// <param name="type">The type of presentation to be overridden</param>
        public PresentationOverrideData(string message, string formattedAmount, PresentationOverrideTypes type)
        {
            Message = message;
            FormattedAmount = formattedAmount;
            Type = type;
        }

        /// <summary>
        ///     The message describing the platform overlay data
        /// </summary>
        public string Message { get; }

        /// <summary>
        ///     The type of presentation to be overridden
        /// </summary>
        public PresentationOverrideTypes Type { get; }

        /// <summary>
        ///     Gets for sets the formatted amount for this transaction
        /// </summary>
        public string FormattedAmount { get; set; }
    }
}
