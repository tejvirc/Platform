namespace Aristocrat.Monaco.Gaming.Contracts
{
    using Kernel;
    using System;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts.Extensions;
    using ProtoBuf;
    using System.CodeDom;

    /// <summary>
    ///     A Game denominations changed event is posted when the active denominations for a game has changed.
    /// </summary>
    [ProtoContract]
    public class GameDenomChangedEvent : BaseEvent
    {
        private const string Delimiter = " - ";

        /// <summary>
        /// Empty constructor for serialization/deserialization
        /// </summary>
        public GameDenomChangedEvent()
        {
        }

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
        [ProtoMember(1)]
        public int GameId { get; }

        /// <summary>
        /// List of affected denoms.
        /// </summary>
        [ProtoMember(2)]
        public string Denoms { get; } = string.Empty;

        /// <summary>
        /// Details of denom change />
        /// </summary>
        [ProtoMember(3)]
        public IGameDetail Details { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, $"{GetType().Name}" + Denoms + Delimiter + Details.ThemeName + Delimiter + Details.PaytableName);
        }
    }
}
