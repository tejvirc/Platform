namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;
    using SharedDevice;
    using static System.FormattableString;

    /// <summary>Definition of the EnabledEvent class.</summary>
    /// <remarks>This event is posted when the printer is enabled.</remarks>
    /// <seealso cref="EnabledReasons" />
    [Serializable]
    public class EnabledEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.
        /// </summary>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(EnabledReasons reasons)
        {
            Reasons = reasons;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnabledEvent" /> class.Initializes a new instance of the EnabledEvent
        ///     class with the printer's ID and enabled reasons.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        /// <param name="reasons">Reasons for the enabled event.</param>
        public EnabledEvent(int printerId, EnabledReasons reasons)
            : base(printerId)
        {
            Reasons = reasons;
        }

        /// <summary>Gets the reasons for the enabled event.</summary>
        public EnabledReasons Reasons { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return Invariant($"{base.ToString()} {Reasons}");
        }
    }
}