namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event to notify that maintenance mode has been exited.
    /// </summary>
    /// <remarks>
    ///     This event will be posted when maintenance mode is exited.
    ///     Typically caused by sending SAS LP0B.
    /// </remarks>
    [ProtoContract]
    public class MaintenanceModeExitedEvent : BaseEvent
    {
    }
}