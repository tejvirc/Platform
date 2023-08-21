namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     A GameExitedNormalEvent is posted when a game is exited due to normal circumstances (no error).
    /// </summary>
    [ProtoContract]
    public class GameExitedNormalEvent : BaseEvent
    {
    }
}