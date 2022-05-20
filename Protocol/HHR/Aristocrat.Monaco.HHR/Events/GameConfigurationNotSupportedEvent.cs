namespace Aristocrat.Monaco.Hhr.Events
{
    using Kernel;

    /// <summary>
    ///     Event will be published when the game has duplicate bets.
    /// </summary>
    public class GameConfigurationNotSupportedEvent : BaseEvent
    {
        public GameConfigurationNotSupportedEvent(int gameId)
        {
            GameId = gameId;
        }

        public int GameId { get; }
    }
}