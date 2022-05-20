namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     The CashOutAbortedEvent is posted after the CashOutStartedEvent if we fail
    ///     to successfully start the TransferOut.  This is probably due to failing to get a Transaction.
    /// </summary>
    [Serializable]
    public class CashOutAbortedEvent : BaseEvent
    {
    }
}