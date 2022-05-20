namespace Aristocrat.Monaco.Sas.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Aristocrat.Sas.Client;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Common;
    using Contracts.SASProperties;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Kernel;
    using Progressive;

    /// <summary>
    ///     Handler for the send extended game N information command
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class LPB5SendExtendedGameNInformationHandler : ISasLongPollHandler<LongPollExtendedGameNInformationResponse,
        LongPollExtendedGameNInformationData>
    {
        private const int MaxNameLength = 19; // We need 1 less than max since we add '-' at then end of the game name

        private readonly IPropertiesManager _propertiesManager;
        private readonly IGameProvider _gameProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;

        /// <summary>
        ///     Create an instance of LPB5SendExtendedGameNInformationHandler
        /// </summary>
        /// <param name="propertiesManager">The properties manager</param>
        /// <param name="gameProvider">The game provider</param>
        /// <param name="protocolLinkedProgressiveAdapter">An instance of <see cref="IProtocolLinkedProgressiveAdapter"/></param>
        public LPB5SendExtendedGameNInformationHandler(
            IPropertiesManager propertiesManager,
            IGameProvider gameProvider,
            IProtocolLinkedProgressiveAdapter protocolLinkedProgressiveAdapter)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(IPropertiesManager));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(IGameProvider));
            _protocolLinkedProgressiveAdapter = protocolLinkedProgressiveAdapter ?? throw new ArgumentNullException(nameof(protocolLinkedProgressiveAdapter));
        }

        /// <inheritdoc />
        public List<LongPoll> Commands { get; } = new List<LongPoll> { LongPoll.SendExtendedGameNInformation };

        /// <inheritdoc />
        public LongPollExtendedGameNInformationResponse Handle(LongPollExtendedGameNInformationData data)
        {
            if (data.TargetDenomination != 0
                && !_gameProvider.GetAllGames().Any(
                    g => g.SupportedDenominations.Contains(data.TargetDenomination.CentsToMillicents())))
            {
                return new LongPollExtendedGameNInformationResponse(0, 0, 0, 0, new List<byte>())
                {
                    ErrorCode = MultiDenomAwareErrorCode.NotValidPlayerDenom
                };
            }

            return data.GameNumber == 0
                ? CreateMachineInfoResponse(data.TargetDenomination.CentsToMillicents())
                : CreateGameInfoResponse((int)data.GameNumber, data.TargetDenomination.CentsToMillicents());
        }

        private static IReadOnlyCollection<byte> GetDenominationCodes(IEnumerable<IDenomination> denoms)
        {
            return denoms
                .Select(denom => DenominationCodes.GetCodeForDenomination((int)denom.Value.MillicentsToCents()))
                .ToList();
        }

        private static int GetMaxBet(IEnumerable<(IGameDetail game, IDenomination denom)> gameEnumerable)
        {
            return gameEnumerable.Where(x => x.denom.Active && x.game.Active)
                .DistinctBy(x => (x.game.Id, x.denom.BetOption, x.denom.LineOption))
                .MaxOrDefault(
                    x => x.game.MaximumWagerCredits(
                        x.game.BetOptionList?.FirstOrDefault(b => b.Name == x.denom.BetOption),
                        x.game.LineOptionList?.FirstOrDefault(l => l.Name == x.denom.LineOption)),
                    0);
        }

        private LongPollExtendedGameNInformationResponse CreateMachineInfoResponse(long denom)
        {
            var gameEnumerable = _gameProvider.GetAllGames().SelectMany(
                    game => game.Denominations.Where(d => denom == 0 || d.Value == denom).Select(d => (game, denom: d)))
                .ToList();

            if (gameEnumerable.Count == 1)
            {
                return CreateGameInfoResponse(gameEnumerable.First().denom.Id, denom);
            }

            var (progressiveId, levels) = GetProgressiveLevels(0, denom);
            if (denom == 0)
            {
                return new LongPollExtendedGameNInformationResponse(
                    GetMaxBet(gameEnumerable),
                    (byte)progressiveId,
                    levels,
                    0,
                    GetDenominationCodes(
                        gameEnumerable.Where(x => x.game.Active).Select(x => x.denom).DistinctBy(x => x.Value)));
            }

            if (gameEnumerable.Count == 0)
            {
                return new LongPollExtendedGameNInformationResponse(0, 0, 0, 0, new List<byte>())
                {
                    ErrorCode = MultiDenomAwareErrorCode.SpecificDenomNotSupported
                };
            }

            return new LongPollExtendedGameNInformationResponse(
                GetMaxBet(gameEnumerable),
                (byte)progressiveId,
                levels,
                0,
                gameEnumerable.Any(x => x.game.Active)
                    ? new List<byte> { DenominationCodes.GetCodeForDenomination(denom.MillicentsToCents()) }
                    : new List<byte>());
        }

        private LongPollExtendedGameNInformationResponse CreateGameInfoResponse(long gameNumber, long denom)
        {
            var (theGame, denomInfo) = _gameProvider.GetGameDetail(gameNumber);
            if (theGame == null || denom != 0 && denom != denomInfo.Value)
            {
                return null;
            }

            var maxBet = denomInfo.Active ? theGame.MaximumWagerCredits(denomInfo) : 0;
            var (progressiveId, levels) = GetProgressiveLevels(theGame.Id, denomInfo.Value);
            var denomString = denomInfo.Value.MillicentsToDollars().ToString(CultureInfo.InvariantCulture);
            var themeName = theGame.ThemeName ?? string.Empty;
            var nameLength = Math.Min(themeName.Length, MaxNameLength - denomString.Length);

            var response = new LongPollExtendedGameNInformationResponse(
                maxBet,
                (byte)progressiveId,
                levels,
                $"{themeName.Substring(0, nameLength)}-{denomString}",
                $"{theGame.PaytableName ?? string.Empty}_{denomString}",
                theGame.WagerCategories.Count(),
                theGame.Active
                    ? new List<byte> { DenominationCodes.GetCodeForDenomination(denomInfo.Value.MillicentsToCents()) }
                    : new List<byte>());
            if (denom != 0 && denom != denomInfo.Value)
            {
                response.ErrorCode = MultiDenomAwareErrorCode.SpecificDenomNotSupported;
            }

            return response;
        }

        private (int progressiveId, uint progressiveLevels) GetProgressiveLevels(int gameId, long denom)
        {
            uint levelIds = 0;
            IEnumerable<IViewableProgressiveLevel> levels;
            if (gameId == 0 || denom == 0)
            {
                levels = _protocolLinkedProgressiveAdapter.ViewConfiguredProgressiveLevels().Where(
                    x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked &&
                         (gameId == 0 || x.GameId == gameId) && (denom == 0 || x.Denomination.Contains(denom)));
            }
            else
            {
                levels = _protocolLinkedProgressiveAdapter.ViewConfiguredProgressiveLevels(gameId, denom).Where(
                    x => x.AssignedProgressiveId.AssignedProgressiveType == AssignableProgressiveType.Linked);
            }

            foreach (var level in levels)
            {
                if (!_protocolLinkedProgressiveAdapter.ViewLinkedProgressiveLevel(
                        level.AssignedProgressiveId.AssignedProgressiveKey,
                        out var linkedLevel) || linkedLevel.ProtocolName != ProgressiveConstants.ProtocolName ||
                    linkedLevel.LevelId <= 0)
                {
                    continue;
                }

                levelIds |= (uint)(1 << (linkedLevel.LevelId - 1));
            }

            return (
                levelIds > 0
                    ? _propertiesManager.GetValue(SasProperties.SasFeatureSettings, new SasFeatures())
                        .ProgressiveGroupId
                    : 0, levelIds);
        }
    }
}