namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using Kernel;

    /// <summary>
    ///     An event for an active progressive game becomes enabled
    /// </summary>
    public class ProgressiveGameEnabledEvent : BaseEvent
    {
        /// <summary>
        ///     Creates the progressive game enabled event
        /// </summary>
        /// <param name="gameId">The game id for this event</param>
        /// <param name="denom">The denom id for this event</param>
        /// <param name="betOption">The bet option for this event</param>
        public ProgressiveGameEnabledEvent(int gameId, long denom, string betOption)
        {
            GameId = gameId;
            Denom = denom;
            BetOption = betOption;
        }

        /// <summary>
        ///     Gets the game id for this event
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the denom id for this event
        /// </summary>
        public long Denom { get; }

        /// <summary>
        ///     Gets the bet option for this event
        /// </summary>
        public string BetOption { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Progressive Game Enabled gameId={GameId} denom={Denom}";
        }
    }
}