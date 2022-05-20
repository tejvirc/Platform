namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     A system driven auto play event is posted to pause or unpause the auto play.
    /// </summary>
    public class AutoPlayPauseRequestedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoPlayPauseRequestedEvent" /> class.
        /// </summary>
        /// <param name="pause"> to indicate to pause/unpause the system driven autoplay. Pausing will stop current
        /// auto play but unpasue will not resume it. It will just allow it if game allows it.</param>
        public AutoPlayPauseRequestedEvent(bool pause)
        {
            Pause = pause;
        }

        /// <summary>
        ///     True to pause autoplay and false to unpause it.
        /// </summary>
        public bool Pause { get; }
    }
}