namespace Aristocrat.Monaco.Accounting.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event emitted when a WAT On transfer has been rejected.
    /// </summary>
    [ProtoContract]
    public class WatOnRejectedEvent : BaseEvent
    {
    }
}