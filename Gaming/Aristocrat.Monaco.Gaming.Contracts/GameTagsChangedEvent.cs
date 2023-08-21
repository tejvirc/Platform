namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;

    /// <summary>
    /// 
    /// </summary>
    public class GameTagsChangedEvent : BaseEvent
    {
        /// <summary>
        /// 
        /// </summary>
        public GameTagsChangedEvent(IGameDetail game)
        {
            Game = game;
        }

        /// <summary>
        /// 
        /// </summary>
        public IGameDetail Game { get; set; }
    }
}
