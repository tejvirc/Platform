namespace Aristocrat.Monaco.G2S.Common.Events
{
    using Aristocrat.G2S.Client;
    using Kernel;

    /// <summary>
    ///     Published when the state changes for cabinet device
    /// </summary>
    public class EgmStateChangedEvent : BaseEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EgmStateChangedEvent" /> class.
        /// </summary>
        /// <param name="egmState">The new EGM state</param>
        public EgmStateChangedEvent(EgmState egmState)
        {
            EgmState = egmState;
        }

        /// <summary>
        ///     Gets the current EGM state
        /// </summary>
        public EgmState EgmState { get; }
    }
}
