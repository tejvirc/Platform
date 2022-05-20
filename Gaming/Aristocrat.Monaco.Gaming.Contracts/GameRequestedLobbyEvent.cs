namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    ///     A GameRequestedLobbyEvent should be posted when game requests lobby function
    /// </summary>
    public class GameRequestedLobbyEvent : BaseEvent
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="immediateAttract">True to start attract immediately, otherwise just go to lobby.</param>
        public GameRequestedLobbyEvent(bool immediateAttract)
        {
            ImmediateAttract = immediateAttract;
        }

        /// <summary>
        ///     True to start attract
        /// </summary>
        public bool ImmediateAttract { get; }
    }
}