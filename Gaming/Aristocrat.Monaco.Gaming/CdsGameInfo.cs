namespace Aristocrat.Monaco.Gaming
{
    using Contracts;

    internal class CdsGameInfo : ICdsGameInfo
    {
        public CdsGameInfo(string id, int minWagerCredits, int maxWagerCredits)
        {
            Id = id;
            MinWagerCredits = minWagerCredits;
            MaxWagerCredits = maxWagerCredits;
        }

        public string Id { get; }

        public int MinWagerCredits { get; }

        public int MaxWagerCredits { get; }
    }
}