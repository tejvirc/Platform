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
        /// 
        /// </summary>
        public int GameId;
        /// <summary>
        /// 
        /// </summary>
        public long Denomination;
    }
}