namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A GameShutdownStartedCompleted is posted when a game is shutdown.
    /// </summary>
    [Serializable]
    public class GameShutdownCompletedEvent : BaseEvent
    {
    }
}