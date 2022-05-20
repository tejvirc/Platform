namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Progressive;

    /// <summary>
    ///     This handles the Send Gaming Machine Id and Send Game N Configuration Commands
    /// </summary>
    public class SendMachineIdAndInfoHandler : ISasLongPollHandler<LongPollMachineIdAndInfoResponse, LongPollGameNConfigurationData>
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private const string GameId = "AT";    // Aristocrat id from Table C-1
        private const string AddId = "000";    // Not supported
        private const byte MinMaxBet = 0x00; // Set to zero for the case we have no games installed
        private const uint GameOptions = 0x00; // Content not strictly defined (left to vendors to define)
        private const int NumberOfRtpDigits = 4;
        private static readonly string DefaultTheoreticalRtp = new string('0', NumberOfRtpDigits);

        /// <summary>
        ///     Instantiates a new instance of the SendGameNConfigurationHandler class
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="gameProvider">The game provider</param>
        /// <param name="protocolLinkedProgressiveAdapter">The linked progressive provider</param>
        public SendMachineIdAndInfoHandler(
            IPropertiesManager propertiesManager,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(IPropertiesManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(IGameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands => new List<LongPoll>
        {
            LongPoll.SendMachineIdAndInformation,
            LongPoll.SendGameNConfiguration
        };

        /// <inheritdoc />
        public LongPollMachineIdAndInfoResponse Handle(LongPollGameNConfigurationData data)
        {
            return data.GameNumber == 0 ?
                CreateMachineInfoResponse(data.AccountingDenom) :
                CreateGameInfoResponse((int)data.GameNumber, data.AccountingDenom);
        }

        private LongPollMachineIdAndInfoResponse CreateMachineInfoResponse(long accountingDenom)
        {
            var response = new LongPollMachineIdAndInfoResponse();
            SetConfiguration(response, accountingDenom, 0, 0);

            var activeGames = _gameProvider.GetEnabledGames().Where(x => x.ActiveDenominations.Any()).ToList();

            response.PaytableId = CreatePaytableId(_gameProvider.GetAllGames()); // Paytable ID is all games installed on the machine.

            if (activeGames.Count > 0)
            {
                // Max Bet, valid range 01-FF. Largest configured bet across all games, or FF if too large.
                // Max bet is only the bet available to the player
                response.MaxBet = (byte)Math.Min(
                    activeGames.Max(game => game.MaximumActiveWagerCredits()),
                    0xFF);

                // Base %, 4 bytes. Average of theoretical payback of max bet across all games, transmitted without decimal (e.g. "90.91%" would be transmitted as "9091").
                // RTP is only the average available to the player
                var theoreticalRtp = 
                    activeGames.Average(game => game.WagerCategories.FirstOrDefault(wc => wc.MaxWagerCredits == game.MaximumWagerCredits)?.TheoPaybackPercent ?? 0).ToMeter();
                response.TheoreticalRtpPercent = $"{(uint)theoreticalRtp % (uint)Math.Pow(10, 4):D4}";
            }

            return response;
        }

        private LongPollMachineIdAndInfoResponse CreateGameInfoResponse(int gameNumber, long accountingDenom)
        {
            var (theGame, denom) = _gameProvider.GetGameDetail(gameNumber);
            if (theGame == null)
            {
                return null;
            }

            var response = new LongPollMachineIdAndInfoResponse();
            SetConfiguration(response, accountingDenom, theGame.Id, denom?.Value ?? 0);

            // Max Bet, valid range 01-FF. Max bet of game, or FF if too large.
            response.MaxBet = (byte)(denom != null ? Math.Min(theGame.MaximumWagerCredits(denom), 0xFF) : 0);
            response.AdditionalId = $"_{theGame.VariationId}";
            response.PaytableId = theGame.PaytableName;

            // the wager category with max bet
            var maxWagerCategory = theGame.WagerCategories.FirstOrDefault(wc => wc.MaxWagerCredits == theGame.MaximumWagerCredits);

            // Base %, 4 bytes. theoretical payback of max bet, transmitted without decimal (e.g. "90.91%" would be transmitted as "9091").
            var theoreticalRtp = (maxWagerCategory?.TheoPaybackPercent ?? 0M).ToMeter();
            response.TheoreticalRtpPercent = $"{(uint)theoreticalRtp % (uint)Math.Pow(10, 4):D4}";

            return response;
        }

        private void SetConfiguration(LongPollMachineIdAndInfoResponse response, long accountingDenom, int gameId, long denom)
        {
            response.GameId = GameId;
            response.AdditionalId = AddId;
            response.Denomination = DenominationCodes.GetCodeForDenomination(accountingDenom);
            response.GameOptions = GameOptions;
            response.MaxBet = MinMaxBet;
            response.TheoreticalRtpPercent = DefaultTheoreticalRtp;

            if (_protocolLinkedProgressiveAdapter.ViewConfiguredProgressiveLevels().Any(
                x => (gameId == 0 || gameId == x.GameId) && (denom == 0 || x.Denomination.Contains(denom)) &&
                     x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                     _protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                         x.AssignedProgressiveId.AssignedProgressiveKey,
                         out var linkedLevel) && linkedLevel.ProtocolName == ProgressiveConstants.ProtocolName))
            {
                response.ProgressiveGroup = (byte)_propertiesManager
                    .GetValue(SasProperties.SasFeatureSettings, new SasFeatures()).ProgressiveGroupId;
            }
            else
            {
                response.ProgressiveGroup = 0;
            }
        }

        private static string CreatePaytableId(IEnumerable<IGameProfile> games)
        {
            // Paytable ID, 6 bytes. Paytable ID needs to be (semi-)unique across game and multi-game configs.
            var checksumData = new List<byte>();
            foreach (var nextGame in games)
            {
                checksumData.AddRange(Encoding.ASCII.GetBytes(nextGame.PaytableId));
                checksumData.AddRange(Encoding.ASCII.GetBytes(nextGame.ThemeId));
            }

            var checksumResult = FletcherChecksum.Checksum16(checksumData);
            // length should be 4 ascii characters for 16-bit number, so no need to check max length
            return $"{checksumResult:X6}";
        }
    }
}
