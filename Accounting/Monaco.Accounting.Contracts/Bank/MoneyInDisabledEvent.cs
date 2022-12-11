namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The MoneyInDisabledEvent is posted when deposits are disabled.
    /// </summary>
    [ProtoContract]
    public class MoneyInDisabledEvent : BaseEvent
    {
    }
}