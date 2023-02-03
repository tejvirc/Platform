namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Post this event when Debug Currency is accepted and is about to be deposited in the bank.
    /// </summary>
    [Serializable]
    public class DebugCurrencyAcceptedEvent : BaseEvent
    {
    }
}
