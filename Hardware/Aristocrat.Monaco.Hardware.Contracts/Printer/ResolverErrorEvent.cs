namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using Properties;
    using System;

    /// <summary>Definition of the ResolverErrorEvent class.</summary>
    /// <remarks>
    ///     The Resolver error event is posted by the TicketContent.Resolver
    ///     if the Resolve method throws an exception in BuildPages() for a requested ticket type.
    /// </remarks>
    [Serializable]
    public class ResolverErrorEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ResolverErrorEvent" /> class.
        /// </summary>
        public ResolverErrorEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResolverErrorEvent" /> class.Initializes a new instance of the
        ///     ResolverErrorEvent class with the printer's ID.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public ResolverErrorEvent(int printerId)
            : base(printerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.PrinterText} {Resources.ResolverErrorText}";
        }
    }
}