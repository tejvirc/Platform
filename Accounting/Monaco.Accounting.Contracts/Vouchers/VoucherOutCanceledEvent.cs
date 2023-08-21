namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event emitted when a VoucherOut has been canceled.
    /// </summary>
    [ProtoContract]
    public class VoucherOutCanceledEvent : BaseEvent
    {
    }
}