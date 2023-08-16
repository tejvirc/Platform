namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     A GameShutdownStartedCompleted is posted when a game is shutdown.
    /// </summary>
    [ProtoContract]
    public class GameShutdownCompletedEvent : BaseEvent
    {
    }
}