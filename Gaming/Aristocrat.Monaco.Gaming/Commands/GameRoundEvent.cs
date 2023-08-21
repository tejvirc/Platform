namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Runtime.Client;

    /// <summary>
    ///     Game round event command
    /// </summary>
    public class GameRoundEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GameRoundEvent" /> class.
        /// </summary>
        /// <param name="state">The game round event type.</param>
        /// <param name="action">The game round event stage.</param>
        /// <param name="playMode">The game round play mode.</param>
        /// <param name="gameRoundInfo">The game round info.</param>
        /// <param name="bet">The bet amount in credits</param>
        /// <param name="win">The win amount in credits.</param>
        /// <param name="stake">The stake in credits.</param>
        /// <param name="data">The recovery blob.</param>
        public GameRoundEvent(
            GameRoundEventState state,
            GameRoundEventAction action,
            PlayMode playMode,
            IList<string> gameRoundInfo,
            ulong bet,
            ulong win,
            ulong stake,
            byte[] data)
        {
            State = state;
            Action = action;
            PlayMode = playMode;
            GameRoundInfo = gameRoundInfo;
            Bet = bet;
            Win = win;
            Stake = stake;
            Data = data;
        }

        /// <summary>
        ///     Gets the game round event state.
        /// </summary>
        public GameRoundEventState State { get; }

        /// <summary>
        ///     Gets the game round event action.
        /// </summary>
        public GameRoundEventAction Action { get; }

        /// <summary>
        ///     Gets the game round play mode.
        /// </summary>
        public PlayMode PlayMode { get; }

        /// <summary>
        ///     Gets the game round info.
        /// </summary>
        public IList<string> GameRoundInfo { get; }

        /// <summary>
        ///     Gets the bet amount in credits.
        /// </summary>
        public ulong Bet { get; }

        /// <summary>
        ///     Gets the win amount in credits.
        /// </summary>
        public ulong Win { get; }

        /// <summary>
        ///     Gets the stake in credits.
        /// </summary>
        public ulong Stake { get; }

        /// <summary>
        ///     Gets the recovery data
        /// </summary>
        public byte[] Data { get; }
    }
}
