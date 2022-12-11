namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     This event is raised when the HardCashLock Key off happens . This is done during WAT Transfer.
    /// </summary>
    [ProtoContract]
    public class HardCashKeyOffEvent : BaseEvent
    {
    }
}