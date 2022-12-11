namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;
    using ProtoBuf;

    /// <summary>
    ///     A GameRequestFailedEvent should be posted by the game in the case that a game has been requested, but is not able
    ///     to be played at this time.
    /// </summary>
    [ProtoContract]
    public class GameRequestFailedEvent : BaseEvent
    {
    }
}