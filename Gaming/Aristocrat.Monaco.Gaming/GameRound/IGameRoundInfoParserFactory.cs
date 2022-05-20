namespace Aristocrat.Monaco.Gaming.GameRound
{
    using System.Collections.Generic;

    public interface IGameRoundInfoParserFactory
    {
        void UpdateGameRoundInfo(IList<string> gameRoundInfo);
    }
}