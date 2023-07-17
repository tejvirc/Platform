namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using System;
    using Kernel;

    /// <summary>
    ///     An event to notify Gaming layer to run a specified game using gameId and Denomination.
    /// </summary>
    [Serializable]
    public class GameLoadRequestedEvent : BaseEvent
    {
        /// <summary>
        /// The GameID to load
        /// </summary>
        public int GameId;
        /// <summary>
        /// The Denomination for the requested game
        /// </summary>
        public long Denomination;
    }
}