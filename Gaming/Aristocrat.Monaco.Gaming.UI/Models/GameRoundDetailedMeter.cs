namespace Aristocrat.Monaco.Gaming.UI.Models
{
    using Application.Contracts;

    /// <summary>
    ///     A single meter's value before game start and at game end
    /// </summary>
    public class GameRoundDetailedMeter
    {
        /// <summary>Localized name of meter</summary>
        public string Name { get; }

        /// <summary>Amount before game start</summary>
        public string BeforeGameStart { get; }

        /// <summary>Amount at game end</summary>
        public string GameEnd { get; }

        /// <summary>Amount before next game</summary>
        public string BeforeNextGame { get; }

        /// <summary>The order in which to display the meter</summary>
        public int Order { get; }

        /// <summary>
        ///     Creates a new GameRoundDetailedMeter
        /// </summary>
        /// <param name="name">The name of the meter</param>
        /// <param name="beforeGameStart">Amount before game start</param>
        /// <param name="gameEnd">Amount at game end</param>
        /// <param name="beforeNextGame">Amount before next game</param>
        /// <param name="order">Order identifier of the meter</param>
        /// <param name="classification">The classification of the meter</param>
        public GameRoundDetailedMeter(string name, long beforeGameStart, long gameEnd, long beforeNextGame, int order, MeterClassification classification)
        {
            Name = name;
            BeforeGameStart = classification.CreateValueString(beforeGameStart);
            GameEnd = classification.CreateValueString(gameEnd);
            BeforeNextGame = classification.CreateValueString(beforeNextGame);
            Order = order;
        }
    }
}
