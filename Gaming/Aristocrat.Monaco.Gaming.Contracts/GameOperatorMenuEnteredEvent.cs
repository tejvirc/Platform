namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A GameOperatorMenuEnteredEvent should be posted by the game when the in-game operator
    ///     menu is first entered.
    /// </summary>
    [Serializable]
    public class GameOperatorMenuEnteredEvent : BaseEvent
    {
    }
}