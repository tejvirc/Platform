namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     A system driven auto play event is posted to start or stop the auto play.
    /// </summary>
    public class AutoPlayRequestedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoPlayRequestedEvent" /> class.
        /// </summary>
        /// <param name="enable"> to indicate to start/stop the system driven autoplay</param>
        public AutoPlayRequestedEvent(bool enable)
        {
            Enable = enable;
        }

        /// <summary>
        ///     True to start system driven autoplay and false to stop it.
        /// </summary>
        public bool Enable { get; }
    }
}