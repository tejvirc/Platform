namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A GameRequestFailedEvent should be posted by the game in the case that a game has been requested, but is not able
    ///     to be played at this time.
    /// </summary>
    [Serializable]
    public class GameRequestFailedEvent : BaseEvent
    {
    }
}