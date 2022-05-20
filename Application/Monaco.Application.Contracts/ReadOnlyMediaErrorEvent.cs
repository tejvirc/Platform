namespace Aristocrat.Monaco.Application.Contracts
{
    using Kernel;
    using static System.FormattableString;

    /// <summary>
    ///     The <see cref="ReadOnlyMediaErrorEvent" /> when an error occurs while validating the read-only media configuration.
    ///     Read-only media is only required in specific jurisdictions.
    /// </summary>
    public class ReadOnlyMediaErrorEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadOnlyMediaErrorEvent" /> class.
        /// </summary>
        /// <param name="reason">Error reason</param>
        public ReadOnlyMediaErrorEvent(string reason)
        {
            Reason = reason;
        }

        /// <summary>
        ///     Gets the event error reason
        /// </summary>
        public string Reason { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} {Reason}");
        }
    }
}