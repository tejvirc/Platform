namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary>Definition of the Printer SelfTestFailedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Printer Service when handling an implementation
    ///     event indicating that a self test has failed.
    /// </remarks>
    [Serializable]
    public class SelfTestFailedEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestFailedEvent"/> class.
        /// </summary>
        public SelfTestFailedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestFailedEvent"/> class.
        /// </summary>
        /// <param name="printerId">The ID of the printer associated with the event.</param>
        public SelfTestFailedEvent(int printerId)
            : base(printerId)
        { }
    }
}
