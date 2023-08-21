namespace Aristocrat.Monaco.Hardware.Contracts.Printer
{
    using System;

    /// <summary>Definition of the Printer SelfTestPassedEvent class.</summary>
    /// <remarks>
    ///     This event is posted by the Printer Service when handling an implementation
    ///     event indicating that a self test has passed.
    /// </remarks>
    [Serializable]
    public class SelfTestPassedEvent : PrinterBaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestPassedEvent"/> class.
        /// </summary>
        public SelfTestPassedEvent()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SelfTestPassedEvent"/> class.
        /// </summary>
        /// <param name="printerId">The ID of the printer associated with the event.</param>
        public SelfTestPassedEvent(int printerId)
            : base(printerId)
        { }
    }
}
