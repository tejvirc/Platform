namespace Aristocrat.Monaco.Gaming.Contracts
{
    /// <summary>
    ///     Definition of a Game Combo
    /// </summary>
    public class GameCombo : IGameCombo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameCombo" /> class.  A game combo is defined by a theme, paytable,
        ///     and denomination.
        /// </summary>
        /// <param name="id">The unique game combo identifier</param>
        /// <param name="gameId">The game identifier</param>
        /// <param name="themeId">Theme associated to the game combo</param>
        /// <param name="paytableId">Paytable associated to the game combo</param>
        /// <param name="denomination">The game denomination</param>
        /// <param name="betOption">The game bet option</param>
        public GameCombo(long id, int gameId, string themeId, string paytableId, long denomination, string betOption)
        {
            Id = id;
            GameId = gameId;
            ThemeId = themeId;
            PaytableId = paytableId;
            Denomination = denomination;
            BetOption = betOption;
        }

        /// <inheritdoc />
        public long Id { get; }

        /// <inheritdoc />
        public int GameId { get; }

        /// <inheritdoc />
        public string ThemeId { get; }

        /// <inheritdoc />
        public string PaytableId { get; }

        /// <inheritdoc />
        public long Denomination { get; }

        /// <inheritdoc />
        public string BetOption { get; }
    }
}
