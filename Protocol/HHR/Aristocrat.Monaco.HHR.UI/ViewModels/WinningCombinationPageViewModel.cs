namespace Aristocrat.Monaco.Hhr.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Hhr.Services;
    using Application.Contracts.Extensions;
    using Client.Messages;
    using Gaming.Contracts;
    using Hardware.Contracts.Button;
    using Kernel;
    using Menu;
    using Models;
    using Command = Menu.Command;
    using Aristocrat.Monaco.Localization.Properties;
    using Aristocrat.Monaco.Application.Contracts.Localization;
    using Kernel.Contracts.MessageDisplay;

    public class WinningCombinationPageViewModel : HhrMenuPageViewModelBase
    {
        private readonly IDictionary<int, IList<WinningPatternModel>> _betPatterns;
        private int _currentPatternIndex;
        private GameInfoResponse _gameInfoResponse;
        private RacePariResponse _racePariResponse;
        private IList<uint> _betOptions;
        private int _currentBetIndex;
        private uint _referenceId;
        private int _numOfLineOptions;

        private readonly IGameDataService _gameDataService;
        private readonly IGameProvider _gameProvider;
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        private readonly ISystemDisableManager _systemDisable;

        public WinningCombinationPageViewModel(
            IGameDataService gameDataService,
            IGameProvider gameProvider,
            IPropertiesManager properties,
            IEventBus eventBus,
            ISystemDisableManager systemDisable)
        {
            _gameDataService = gameDataService ?? throw new ArgumentNullException(nameof(gameDataService));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _systemDisable = systemDisable ?? throw new ArgumentNullException(nameof(systemDisable));
            _betPatterns = new Dictionary<int, IList<WinningPatternModel>>();
            ShowFooterText = false;
        }

        public override async Task Init(Command command)
        {
            if (Commands.Count == 0)
            {
                Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Bet));
                Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.ExitHelp));
                Commands.Add(new HhrPageCommand(PageCommandHandler, true, Command.Next));

                Logger.Debug("Buttons are added to Winning Combination Page");
            }

            _betPatterns.Clear();
            var gameId = _properties.GetValue(GamingConstants.SelectedGameId, 0);
            var gameDetail = _gameProvider.GetGame(gameId);
            _referenceId = Convert.ToUInt32(gameDetail?.ReferenceId);

            _systemDisable.Disable(
                HhrUiConstants.WaitingForWinningCombinationInfo,
                SystemDisablePriority.Immediate,
                ResourceKeys.WaitingForWinningCombinationInfo,
                CultureProviderType.Operator);

            _gameInfoResponse = (await _gameDataService.GetGameInfo()).FirstOrDefault(
                x => x.GameId == _referenceId);

            if (_gameInfoResponse == null)
            {
                Logger.Warn("Failed to get game information from HHR Server");
                _systemDisable.Enable(HhrUiConstants.WaitingForWinningCombinationInfo);
                return;
            }

            _racePariResponse = await _gameDataService.GetRaceInformation(_referenceId, 0, 0);
            if (_racePariResponse == null)
            {
                Logger.Warn("Failed to get race information from HHR Server");
                _systemDisable.Enable(HhrUiConstants.WaitingForWinningCombinationInfo);
                return;
            }

            _betOptions = _gameInfoResponse.RaceTicketSets.TicketSet.Where(item => item.Credits != 0)
                .Select(item => item.Credits).ToList();

            var betOption = Convert.ToUInt32(_properties.GetValue(GamingConstants.SelectedBetCredits, (long)0));

            _currentBetIndex = _betOptions.IndexOf(betOption);
            _currentPatternIndex = 0;

            _numOfLineOptions = gameDetail?.ActiveLineOption?.Lines?.Count() ?? 1;

            //get the combination patterns for all the bets
            for (var betIndex = 0; betIndex < _betOptions.Count; betIndex++)
            {
                IList<WinningPatternModel> winningPatterns = new List<WinningPatternModel>();
                var patterns = _gameInfoResponse.RaceTicketSets.TicketSet
                    .FirstOrDefault(ts => ts.Credits == _betOptions[betIndex]).Pattern;

                for (var index = 0; index < patterns.Length; index++)
                {
                    if (patterns[index].RaceGroup != 1 && patterns[index].RaceGroup != 2)
                    {
                        continue;
                    }

                    var prizeLocationIndex = await _gameDataService.GetPrizeLocationForAPattern(
                        _gameInfoResponse.GameId,
                        _betOptions[betIndex],
                        index);

                    if (prizeLocationIndex == -1)
                    {
                        Logger.Warn("Failed to get prize location index used for extra winnings");
                        continue;
                    }

                    winningPatterns.Add(
                        new WinningPatternModel
                        {
                            Wager =
                                long.Parse(patterns[index].Prize.GetPrizeString(HhrConstants.Wager)).CentsToDollars()
                                    .FormattedCurrencyString(),
                            RaceSet = patterns[index].RaceGroup,
                            IncludesProgressiveResetValues =
                                !string.IsNullOrEmpty(
                                    patterns[index].Prize.GetPrizeString(HhrConstants.ProgressiveInformation)),
                            GuaranteedCredits = patterns[index].PrizeValue / _gameInfoResponse.Denomination,
                            Patterns = new[]
                            {
                                patterns[index].Pattern1, patterns[index].Pattern2, patterns[index].Pattern3,
                                patterns[index].Pattern4, patterns[index].Pattern5
                            },
                            ExtraWinnings =
                                (double)((long)_racePariResponse.TemplatePool[betIndex]
                                    .PrizeDataRace[prizeLocationIndex]).CentsToDollars()
                        });
                }

                winningPatterns = winningPatterns.OrderBy(o => o.RaceSet).ThenByDescending(o => o.GuaranteedCredits)
                    .ToList();

                Logger.Debug($"Total patterns count  for bet {_betOptions[betIndex]} is {winningPatterns.Count}");
                for (var i = 0; i < winningPatterns.Count; i++)
                {
                    Logger.Debug(
                        $" Pattern #{i + 1}: Set: {winningPatterns[i].RaceSet}, Bet: {_betOptions[betIndex]}, GuaranteedCredits: {winningPatterns[i].GuaranteedCredits}, Extra: {winningPatterns[i].ExtraWinnings}, Prog: {winningPatterns[i].IncludesProgressiveResetValues}");
                }

                _betPatterns.Add(betIndex, winningPatterns);
                RaisePropertyChanged(nameof(CurrentWinningPattern));
            }

            _systemDisable.Enable(HhrUiConstants.WaitingForWinningCombinationInfo);
        }

        public WinningPatternModel CurrentWinningPattern =>
            _betPatterns[_currentBetIndex].Count > 0
                ? _betPatterns[_currentBetIndex][_currentPatternIndex]
                : new WinningPatternModel();

        public string CurrentWinningPatternExtraWinnings
        {
            get
            {
                string format = Localizer.GetString(ResourceKeys.ExtraWinningsStringFormat, CultureProviderType.Player);
                return string.Format(format, CurrentWinningPattern.ExtraWinnings.FormattedCurrencyString());
            }
        }

        public string CurrentWinningPatternGuaranteedCredits
        {
            get
            {
                string format = Localizer.GetString(ResourceKeys.GuaranteedCreditsStringFormat, CultureProviderType.Player);
                return string.Format(format, CurrentWinningPattern.GuaranteedCredits);
            }
        }

        public string IncludesProgressiveResetValues => Localizer.GetString(ResourceKeys.IncludesProgressiveResetValues, CultureProviderType.Player);

        public string WagerStringFormat => Localizer.GetString(ResourceKeys.WagerStringFormat, CultureProviderType.Player);

        private void PageCommandHandler(object command)
        {
            switch ((Command)command)
            {
                case Command.BetUp:
                    BetUp();
                    break;
                case Command.BetDown:
                    BetDown();
                    break;
                case Command.Next:
                    NextPattern();
                    break;
            }
            OnHhrButtonClicked((Command)command);
        }

        private void BetUp()
        {
            if (HostPageViewModelManager.BetButtonDelayLimit())
            {
                _eventBus.Publish(new DownEvent((int)ButtonLogicalId.BetUp));
                OnHhrButtonClicked(Command.BetUp);

                _currentBetIndex += _numOfLineOptions;
                if (_currentBetIndex >= _betOptions.Count)
                {
                    _currentBetIndex -= _betOptions.Count;
                }
                _currentPatternIndex = 0;
                RaisePropertyChanged(nameof(CurrentWinningPattern));
            }
        }

        private void BetDown()
        {
            if (HostPageViewModelManager.BetButtonDelayLimit())
            {
                _eventBus.Publish(new DownEvent((int)ButtonLogicalId.BetDown));
                OnHhrButtonClicked(Command.BetDown);

                if (_currentBetIndex < _numOfLineOptions)
                {
                    return;
                }

                _currentBetIndex -= _numOfLineOptions;
                _currentPatternIndex = 0;
                RaisePropertyChanged(nameof(CurrentWinningPattern));
            }
        }

        private void NextPattern()
        {
            //Exit Help if this the last page and player click the Next Button
            if (_betPatterns.Count == 0 || _currentPatternIndex == _betPatterns[_currentBetIndex].Count - 1)
            {
                OnHhrButtonClicked(Command.ExitHelp);
                return;
            }

            _currentPatternIndex++;
            RaisePropertyChanged(nameof(CurrentWinningPattern));
        }

        public override void Reset()
        {
            base.Reset();
            _betPatterns.Clear();
        }
    }
}