namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     An event to notify that maintenance mode has been entered.
    /// </summary>
    /// <remarks>
    ///     This event will be posted when maintenance mode is entered.
    ///     Typically caused by sending SAS LP0A.
    /// </remarks>
    [ProtoContract]
    public class MaintenanceModeEnteredEvent : BaseEvent
    {
    }
}