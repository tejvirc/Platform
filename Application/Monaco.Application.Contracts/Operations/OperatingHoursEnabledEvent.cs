namespace Aristocrat.Monaco.Application.Contracts.Operations
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event raised when the operating hours schedule indicates the cabinet should be enabled.
    /// </summary>
    [ProtoContract]
    public class OperatingHoursEnabledEvent : BaseEvent
    {
    }
}