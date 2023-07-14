namespace Aristocrat.Monaco.Gaming.Contracts.Events
{
    using System;
    using Kernel;

    /// <summary>
    /// 
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