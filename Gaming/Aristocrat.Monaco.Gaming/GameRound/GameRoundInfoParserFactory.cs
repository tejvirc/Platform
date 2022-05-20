namespace Aristocrat.Monaco.Gaming.GameRound
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks.Dataflow;
    using log4net;

    public class GameRoundInfoParserFactory : IGameRoundInfoParserFactory
    {
        private const int MinimumTriggeredDataSize = 3;
        private const int GameTypeOffset = 0;
        private const int VersionOffset = 1;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IReadOnlyDictionary<(string, string), IGameRoundInfoParser> _gameRoundParsers;
        private readonly ActionBlock<IList<string>> _gameRoundProcessor;

        public GameRoundInfoParserFactory(IEnumerable<IGameRoundInfoParser> parsers)
        {
            _gameRoundParsers = parsers.ToDictionary(x => (x.GameType, x.Version), x => x);
            _gameRoundProcessor = new ActionBlock<IList<string>>(ProcessGameRoundInfo);
        }

        public void UpdateGameRoundInfo(IList<string> gameRoundInfo)
        {
            _gameRoundProcessor.Post(gameRoundInfo);
        }

        private void ProcessGameRoundInfo(IList<string> gameRoundInfo)
        {
            // We could be getting either a Scene change notification or a Poker Hand update

            // Scene change notifications will be encoded as follows:
            // gameRoundInfo[0] contains gameType ("Controllable Scene")
            // gameRoundInfo[1] contains version  ("1")
            // gameRoundInfo[2] will contain the scene name, for example "RSFS" or "Normal"

            // Poker hand updates will be encoded as follows:
            // gameRoundInfo[0] contains gameType ("poker")
            // gameRoundInfo[1] contains version  ("1")
            // gameRoundInfo[2] contains game information encoded in a json string
            // optional: gameRoundInfo[3] and higher contain additional game information encoded in a string
            if (gameRoundInfo.Count < MinimumTriggeredDataSize)
            {
                return;
            }

            var gameType = gameRoundInfo[GameTypeOffset];
            var version = gameRoundInfo[VersionOffset];
            if (_gameRoundParsers.TryGetValue((gameType, version), out var parser))
            {
                parser.UpdateGameRoundInfo(gameRoundInfo);
            }
            else
            {
                Logger.Warn($"No valid process found for game type: {gameType}, version: {version}");
            }
        }
    }
}