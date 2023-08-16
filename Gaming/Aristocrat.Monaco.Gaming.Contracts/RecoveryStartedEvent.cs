namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Raised when recovery starts.
    /// </summary>
    [ProtoContract]
    public class RecoveryStartedEvent : BaseEvent
    {
    }
}