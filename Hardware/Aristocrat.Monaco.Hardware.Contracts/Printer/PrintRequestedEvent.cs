namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;
    using static System.FormattableString;

    /// <summary>
    ///     Definition of the PrintRequestEvent
    /// </summary>
    /// <remarks>
    ///     The PrintRequestedEvent is posted by the PrintService in response to a Print request.  This event occurs
    ///     before the Ticket content is resolved and the PrintRaw is called on the printer implementation.
    /// <seealso cref="PrintStartedEvent"/>
    /// </remarks>
    [Serializable]
    public class PrintRequestedEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrintRequestedEvent"/> class.
        /// </summary>
        /// <param name="printerId">The associated printer's Id.</param>
        /// <param name="templateId">The template Id being printed.</param>
        public PrintRequestedEvent(int printerId, int templateId)
            : base(printerId)
        {
            TemplateId = templateId;
        }

        /// <summary>
        ///     Gets the template Id being printed
        /// </summary>
        public int TemplateId { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} [TemplateId={TemplateId}]");
        }
    }
}
