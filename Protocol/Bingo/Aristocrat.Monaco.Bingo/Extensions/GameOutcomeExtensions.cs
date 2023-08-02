namespace Aristocrat.Monaco.Bingo.Extensions
{
    using System.Linq;
    using Aristocrat.Bingo.Client.Messages.GamePlay;

    public static class GameOutcomeExtensions
    {
        /// <summary>
        ///     Indicates if we have a golden card in an outcome
        /// </summary>
        /// <param name="outcome">The outcome to check for a golden card</param>
        /// <returns>true if a golden card, false otherwise</returns>
        public static bool IsGolden(this GameOutcome outcome)
        {
            return outcome.GameIndex != 0 && outcome.BingoDetails.CardsPlayed.Any(card => card.IsGolden);
        }
    }
}