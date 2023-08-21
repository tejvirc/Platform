namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event to notify that the Audit Ticket Creator is Initialized.
    /// </summary>
    [ProtoContract]
    public class AuditTicketCreatorInitializedEvent : BaseEvent
    {
    }
}