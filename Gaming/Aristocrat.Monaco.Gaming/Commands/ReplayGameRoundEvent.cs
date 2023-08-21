namespace Aristocrat.Monaco.Gaming.Commands
{
    using System.Collections.Generic;
    using Runtime.Client;

    public class ReplayGameRoundEvent
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ReplayGameRoundEvent" /> class.
        /// </summary>
        /// <param name="state">The game round event type.</param>
        /// <param name="action">The game round event stage.</param>
        /// <param name="gameRoundInfo">The game round info.</param>
        public ReplayGameRoundEvent(
            GameRoundEventState state,
            GameRoundEventAction action,
            IList<string> gameRoundInfo)
        {
            State = state;
            Action = action;
            GameRoundInfo = gameRoundInfo;
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
        ///     Gets the game round info.
        /// </summary>
        public IList<string> GameRoundInfo { get; }
    }
}
