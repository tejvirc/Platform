namespace Aristocrat.Monaco.Application.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     Event that gets published when Exit Reserve Button is pressed.
    /// </summary>
    [ProtoContract]
    public class ExitReserveButtonPressedEvent : BaseEvent
    {
    }
}
