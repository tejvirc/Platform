namespace Aristocrat.Bingo.Client.Messages.GamePlay
{
    using System.Collections.Generic;

    public class GameOutcomeBingoDetails
    {
        public GameOutcomeBingoDetails(
            int gameEndWinEligibility,
            IReadOnlyCollection<CardPlayed> cardsPlayed,
            IReadOnlyCollection<int> ballCall,
            int joinBallNumber)
        {
            GameEndWinEligibility = gameEndWinEligibility;
            CardsPlayed = cardsPlayed;
            BallCall = ballCall;
            JoinBallNumber = joinBallNumber;
        }

        public int GameEndWinEligibility { get; }

        public IReadOnlyCollection<CardPlayed> CardsPlayed { get; }

        public IReadOnlyCollection<int> BallCall { get; }

        public int JoinBallNumber { get; }
    }
}