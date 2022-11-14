namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using Aristocrat.Monaco.Kernel;

    /// <summary>
    /// An event published when the LobbyClock wants to flash.
    /// Published from LobbyClockService
    /// </summary>
    public class LobbyClockFlashChangedEvent : BaseEvent
    {
        /// <summary>
        /// Event to Start a flash event for the lobby clock
        /// </summary>
        public LobbyClockFlashChangedEvent()
        {
        }
    }
}