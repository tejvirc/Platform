namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The TransferOnDisabledOverlayEvent is posted when FundsTransferDisable on overlay become false
    /// </summary>
    [ProtoContract]
    public class TransferEnableOnOverlayEvent : BaseEvent {}
}