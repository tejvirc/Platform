namespace Aristocrat.Monaco.G2S.Handlers.GamePlay
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;

    /// <summary>
    ///     Defines a new instance of an <see cref="ICommandHandler{TClass,TCommand}" />
    /// </summary>
    public class GetGamePlayProfile : ICommandHandler<gamePlay, getGamePlayProfile>
    {
        private readonly IG2SEgm _egm;
        private readonly IGameHistory _gameHistory;
        private readonly IProtocolLinkedProgressiveAdapter _progressives;
        private readonly IGameProvider _provider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GetGamePlayProfile" /> class.
        /// </summary>
        public GetGamePlayProfile(IG2SEgm egm, IGameProvider provider, IGameHistory gameHistory, IProtocolLinkedProgressiveAdapter progressives)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _progressives = progressives ?? throw new ArgumentNullException(nameof(progressives));
        }

        /// <inheritdoc />
        public async Task<Error> Verify(ClassCommand<gamePlay, getGamePlayProfile> command)
        {
            return await Sanction.OwnerAndGuests<IGamePlayDevice>(_egm, command);
        }

        /// <inheritdoc />
        public async Task Handle(ClassCommand<gamePlay, getGamePlayProfile> command)
        {
            var device = _egm.GetDevice<IGamePlayDevice>(command.Class.deviceId);

            var response = command.GenerateResponse<gamePlayProfile>();

            var game = _provider.GetGame(device.Id);

            response.Command.configurationId = device.ConfigurationId;
            response.Command.restartStatus = device.RestartStatus;
            response.Command.useDefaultConfig = device.UseDefaultConfig;
            response.Command.requiredForPlay = device.RequiredForPlay;
            response.Command.minLogEntries = _gameHistory.MaxEntries;
            response.Command.themeId = game.ThemeId;
            response.Command.paytableId = game.PaytableId;
            response.Command.maxWagerCredits = game.MaximumWagerCredits;
            response.Command.progAllowed = _progressives.ViewProgressiveLevels().Any(p => p.GameId == game.Id);
            response.Command.secondaryAllowed = game.Denominations.Any(d => d.Active);
            response.Command.centralAllowed = game.CentralAllowed;
            response.Command.configDateTime = device.ConfigDateTime;
            response.Command.configComplete = device.ConfigComplete;
            response.Command.maxPaybackPct = game.MaximumPaybackPercent.ToMeter();
            response.Command.minPaybackPct = game.MinimumPaybackPercent.ToMeter();

            int variation = 0;
            variation = Int32.Parse(game.VariationId);
            response.Command.variation = variation;
            response.Command.linkThemeId = game.ThemeId;

            response.Command.denomMeterType = device.DenomMeterType;
            response.Command.themeName = game.ThemeName;
            response.Command.paytableName = game.PaytableName;

            response.Command.setAccessViaConfig = device.SetViaAccessConfig;
            response.Command.accessibleGame =
                (game.Status & GameStatus.DisabledByBackend) != GameStatus.DisabledByBackend &&
                game.ActiveDenominations.Any();
            response.Command.standardPlay = true;
            response.Command.tournamentPlay = false;

            response.Command.wagerCategoryList = new wagerCategoryList
            {
                wagerCategoryItem =
                    game.WagerCategories.Select(
                        c => new wagerCategoryItem
                        {
                            wagerCategory = c.Id,
                            theoPaybackPct = c.TheoPaybackPercent.ToMeter(),
                            minWagerCredits = c.MinWagerCredits ?? 1,
                            maxWagerCredits = c.MaxWagerCredits ?? 1
                        }).ToArray()
            };

            response.Command.winLevelList = new winLevelList
            {
                winLevelItem =
                    game.WinLevels.Select(l => new winLevelItem
                    {
                        progressiveAllowed = l.ProgressiveAllowed,
                        winLevelCombo = l.WinLevelCombo,
                        winLevelIndex = l.WinLevelIndex
                    }).ToArray()
            };

            if(response.Command.winLevelList.winLevelItem.Length == 0)
            {
                response.Command.EmptyWinLevelList();
            }

            await Task.CompletedTask;
        }
    }
}