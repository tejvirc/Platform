namespace Aristocrat.Monaco.Gaming.Contracts.Progressives
{
    using Kernel;

    /// <summary>
    ///     An event for an active progressive game becomes disabled for any reason
    /// </summary>
    public class ProgressiveGameDisabledEvent : BaseEvent
    {
        /// <summary>
        ///     Creates the progressive game disabled event
        /// </summary>
        /// <param name="gameId">The game id for this event</param>
        /// <param name="denom">The denom id for this event</param>
        /// <param name="betOption">The bet option for this progressive game disable</param>
        public ProgressiveGameDisabledEvent(int gameId, long denom, string betOption)
        {
            GameId = gameId;
            Denom = denom;
            BetOption = betOption;
        }

        /// <summary>
        ///     Gets the game id for the disabled progressive game
        /// </summary>
        public int GameId { get; }

        /// <summary>
        ///     Gets the denom for the disabled progressive game
        /// </summary>
        public long Denom { get; }

        /// <summary>
        ///     Gets the bet option for the disabled progressive game
        /// </summary>
        public string BetOption { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Progressive Game Disabled gameId={GameId}, denom={Denom}, betOption={BetOption}";
        }
    }
}