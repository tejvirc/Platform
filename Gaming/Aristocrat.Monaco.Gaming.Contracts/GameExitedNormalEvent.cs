namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     A GameExitedNormalEvent is posted when a game is exited due to normal circumstances (no error).
    /// </summary>
    [Serializable]
    public class GameExitedNormalEvent : BaseEvent
    {
    }
}