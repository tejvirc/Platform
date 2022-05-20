namespace Aristocrat.Monaco.Gaming.Commands
{
    public class Wager
    {
        public Wager(int gameId, long denomination, long amount)
        {
            GameId = gameId;
            Denomination = denomination;
            Amount = amount;
        }

        public int GameId { get; }

        public long Denomination { get; }

        public long Amount { get; }
    }
}