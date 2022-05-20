namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using Properties;
    using System;

    /// <summary> Definition of the LoadingRegionsAndTemplatesEvent class.</summary>
    [Serializable]
    public class LoadingRegionsAndTemplatesEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="LoadingRegionsAndTemplatesEvent" /> class.
        /// </summary>
        public LoadingRegionsAndTemplatesEvent()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LoadingRegionsAndTemplatesEvent" /> class.
        /// </summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public LoadingRegionsAndTemplatesEvent(int printerId)
            : base(printerId)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Resources.PrinterText} {Resources.TransferStatusErrorText} {Resources.ClearedText}";
        }
    }
}