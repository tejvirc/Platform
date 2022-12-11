namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The MoneyInEnabledEvent is posted when deposits are enabled.
    /// </summary>
    [ProtoContract]
    public class MoneyInEnabledEvent : BaseEvent
    {
    }
}