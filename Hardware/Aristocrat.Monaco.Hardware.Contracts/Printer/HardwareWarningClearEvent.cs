namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using Properties;
    using System;
    using static System.FormattableString;

    /// <summary> Definition of the HardwareWarningClearEvent class.</summary>
    /// <remarks>
    ///     This event is posted if the printer status bits have the head error bit set.
    ///     This indicates the printer head has failed and requires service.
    /// </remarks>
    [Serializable]
    public class HardwareWarningClearEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareWarningClearEvent"/> class.
        /// </summary>
        /// <param name="warning">The warning.</param>
        public HardwareWarningClearEvent(PrinterWarningTypes warning)
        {
            Warning = warning;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="HardwareWarningClearEvent"/> class.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        /// <param name="warning">The warning.</param>
        public HardwareWarningClearEvent(int printerId, PrinterWarningTypes warning)
            : base(printerId)
        {
            Warning = warning;
        }

        /// <summary>Gets the warning.</summary>
        /// <value>The warning.</value>
        public PrinterWarningTypes Warning { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            if (PrinterEventsDescriptor.WarningTexts.TryGetValue(Warning, out var result) && !string.IsNullOrEmpty(result))
            {
                return result + $" {Resources.ClearedText}";
            }

            return Invariant($"{base.ToString()} [Warning={Warning}]");
        }
    }
}
