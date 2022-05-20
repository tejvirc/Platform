namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Provides status on whether auto play is active or not and
    ///     allows starting or stopping auto play
    /// </summary>
    public interface IAutoPlayStatusProvider
    {
        /// <summary>
        ///     If auto play is active, turn it off. Forces off any player initiated autoplay.
        /// </summary>
        /// <returns>True if auto play was active, false otherwise.</returns>
        bool EndAutoPlayIfActive();

        /// <summary>
        ///     Requests the game to start system auto play.
        /// </summary>
        void StartSystemAutoPlay();

        /// <summary>
        ///     Requests the game to stop system auto play.
        /// </summary>
        void StopSystemAutoPlay();

        /// <summary>
        ///     Requests the game to pause player auto play.
        /// </summary>
        void PausePlayerAutoPlay();

        /// <summary>
        ///     Requests the game to unpause player auto play.
        /// </summary>
        void UnpausePlayerAutoPlay();
    }
}