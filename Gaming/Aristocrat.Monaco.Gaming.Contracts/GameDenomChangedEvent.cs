namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;
    using System;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts.Extensions;

    /// <summary>
    ///     A Game denominations changed event is posted when the active denominations for a game has changed.
    /// </summary>
    [Serializable]
    public class GameDenomChangedEvent : BaseEvent
    {
        private const string Delimiter = " - ";

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDenomChangedEvent"/> class.
        /// </summary>
        /// <param name="gameId">Game ID.</param>
        /// <param name="details">Game details</param>
        /// <param name="includeDenoms">Whether to include denoms in event string.</param>
        /// <param name="multiplier">Denom multiplier.</param>
        public GameDenomChangedEvent(int gameId, IGameDetail details, bool includeDenoms, double multiplier)
        {
            GameId = gameId;
            Details = details;
            if (includeDenoms)
            {
                Denoms = Delimiter + string.Join("+", Details.ActiveDenominations.Select(d => (d / multiplier).FormattedCurrencyString()));
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameDenomChangedEvent"/> class.
        /// </summary>
        /// <param name="gameId">Game ID.</param>
        /// <param name="details">Game details</param>
        /// <param name="denom">A single denom value.</param>
        /// <param name="multiplier">Denom multiplier.</param>
        public GameDenomChangedEvent(int gameId, IGameDetail details, long denom, double multiplier)
        {
            GameId = gameId;
            Details = details;
            Denoms = Delimiter + (denom / multiplier).FormattedCurrencyString();
        }

        /// <summary>
        ///     Gets the Game ID of the target game.
        /// </summary>
        public int GameId { get; }

        /// <summary>
        /// List of affected denoms.
        /// </summary>
        public string Denoms { get; } = string.Empty;

        /// Details of denom change />
        public IGameDetail Details { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, $"{GetType().Name}" + Denoms + Delimiter + Details.ThemeName + Delimiter + Details.PaytableName);
        }
    }
}
