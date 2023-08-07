namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Collections.Generic;

    /// <summary>
    ///     Holds outcomes for multiple games
    /// </summary>
    public class GameOutcomes : IResponse
    {
        public GameOutcomes(ResponseCode code, IEnumerable<GameOutcome> outcomes)
        {
            ResponseCode = code;
            Outcomes = outcomes;
        }

        /// <inheritdoc/>
        public ResponseCode ResponseCode { get; }

        /// <summary>
        ///     Contains the outcomes for one or more games
        /// </summary>
        public IEnumerable<GameOutcome> Outcomes { get; }
    }
}
