    namespace Aristocrat.Monaco.Gaming.Contracts
{
    using System;
    using Kernel;

    /// <summary>
    ///     An End Game Process event is posted to terminate the current game process.
    /// </summary>
    [Serializable]
    public class TerminateGameProcessEvent : BaseEvent
    {
    }
}