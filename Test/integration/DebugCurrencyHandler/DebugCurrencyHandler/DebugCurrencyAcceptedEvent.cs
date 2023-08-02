namespace Vgt.Client12.Testing.Tools
{
    using System;
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    ///     Post this event when Debug Currency is accepted and is about to be deposited in the bank.
    /// </summary>
    [Serializable]
    public class DebugCurrencyAcceptedEvent : BaseEvent
    {
        /// <summary>
        ///     Debug Currency Accepted Event
        /// </summary>
        /// <param name="previousBalance">Bank balance before the debug currency is accepted</param>
        public DebugCurrencyAcceptedEvent(long previousBalance)
        {
            PreviousBalance = previousBalance;
        }

        /// <summary>
        ///     The value of the bank from before the debug currency was accepted
        /// </summary>
        public long PreviousBalance { get; }
    }
}