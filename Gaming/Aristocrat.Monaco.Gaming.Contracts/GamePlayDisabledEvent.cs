namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     A GamePlayDisabledEvent should be posted whenever game play is disabled.
    /// </summary>
    [ProtoContract]
    public class GamePlayDisabledEvent : BaseEvent
    {
    }
}