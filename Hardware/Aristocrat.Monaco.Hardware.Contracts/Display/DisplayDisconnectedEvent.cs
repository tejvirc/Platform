namespace Aristocrat.Monaco.Hardware.Contracts.Display
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     This event is fired whenever a display is disconnected
    /// </summary>
    [ProtoContract]
    public class DisplayDisconnectedEvent : BaseEvent
    {
    }
}