namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     Event emitted when a pending handpay request has been cancelled.
    /// </summary>
    /// <remarks>
    /// This happens when the player decides to exit credit redemption lock up state after instigating a handpay.
    /// </remarks>
    [Serializable]
    public class HandpayPendingCanceledEvent : BaseEvent
    {
    }
}