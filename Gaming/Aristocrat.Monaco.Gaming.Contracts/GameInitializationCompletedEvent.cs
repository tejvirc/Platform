namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A GameInitializationCompletedEvent should be posted by the game, once the game has
    ///     finished initializing and is idle and waiting for player input.  For example, a
    ///     reel spinning game would want to finish homing the reels before posting this event.
    /// </summary>
    [Serializable]
    public class GameInitializationCompletedEvent : BaseEvent
    {
    }
}