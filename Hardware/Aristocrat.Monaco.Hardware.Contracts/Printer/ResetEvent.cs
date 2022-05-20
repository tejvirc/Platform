namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary>Definition of the ResetEvent class.</summary>
    /// <remarks>This event is posted by the Printer implementation whenever there is a need to rediscover the printer.</remarks>
    [Serializable]
    public class ResetEvent : PrinterBaseEvent
    {
        /// <summary>Initializes a new instance of the <see cref="ResetEvent" /> class.</summary>
        public ResetEvent()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ResetEvent" /> class with the printer's ID.</summary>
        /// <param name="printerId">The associated printer's ID.</param>
        public ResetEvent(int printerId)
            : base(printerId)
        {
        }
    }
}
