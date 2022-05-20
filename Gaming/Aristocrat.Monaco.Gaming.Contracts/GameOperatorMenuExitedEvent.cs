namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A GameOperatorMenuExitedEvent should be posted by the game when the in-game operator
    ///     menu exits.
    /// </summary>
    [Serializable]
    public class GameOperatorMenuExitedEvent : BaseEvent
    {
    }
}