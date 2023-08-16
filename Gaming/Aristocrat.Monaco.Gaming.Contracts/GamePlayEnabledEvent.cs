namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     The GamePlayEnabledEvent should be posted whenever game play is enabled.
    /// </summary>
    [ProtoContract]
    public class GamePlayEnabledEvent : BaseEvent
    {
    }
}