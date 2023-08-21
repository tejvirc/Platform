namespace Aristocrat.Monaco.Gaming.Commands.RuntimeEvents
{
    /// <summary>
    ///     Interface for a replay event handler.
    /// </summary>
    public interface IReplayRuntimeEventHandler
    {
        /// <summary>
        ///     Handles/processes the ReplayGameRoundEvent event.
        /// </summary>
        void HandleEvent(ReplayGameRoundEvent replayGameRoundEvent);
    }
}
