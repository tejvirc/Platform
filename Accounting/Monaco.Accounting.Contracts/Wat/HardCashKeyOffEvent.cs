namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     This event is raised when the HardCashLock Key off happens . This is done during WAT Transfer.
    /// </summary>
    [Serializable]
    public class HardCashKeyOffEvent : BaseEvent
    {
    }
}