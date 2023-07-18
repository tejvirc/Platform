namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Accounting.Contracts;
    using Accounting.Contracts.HandCount;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Events;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Central;
    using Contracts.Events.OperatorMenu;
    using Contracts.Lobby;
    using Contracts.Models;
    using Contracts.Progressives;
    using Contracts.Progressives.Linked;
    using Contracts.Tickets;
    using DetailedGameMeters;
    using Diagnostics;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.Reel;
    using Hardware.Contracts.Reel.Events;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using Models;
    using Monaco.UI.Common.Models;
    using Tickets;
    using Views.OperatorMenu;

    /// <summary>
    ///     Defines the GameHistoryViewModel class
    /// </summary>
    [CLSCompliant(false)]
    public class GameHistoryViewModel : OperatorMenuDiagnosticPageViewModelBase
    {
        private const int MaxGameRoundInfoTextLength = 140;
        private const string GameDescriptionDelimiter = "\n\n";
        private const char NewLineChar = '\n';
        private readonly int _eventsPerPage;
        private readonly bool _meterFreeGamesIndependently;

        private readonly ILobbyStateManager _lobbyStateManager;
        private readonly IGameHistory _gameHistoryProvider;
        private readonly IGameRecovery _gameRecovery;
        private readonly IGameDiagnostics _gameDiagnostics;
        private readonly IGamePlayState _gamePlayState;
        private readonly IBank _bank;
        private readonly IDialogService _dialogService;
        private readonly IGameHistoryLog[] _gameLogs;

        private ObservableCollection<GameRoundHistoryItem> _gameHistory;
        private bool _isReplaying;
        private GameRoundHistoryItem _selectedGameItem;
        private readonly IProgressiveLevelProvider _progressiveProvider;
        private readonly IPropertiesManager _propertiesManager;
        private readonly ICentralProvider _centralProvider;
        private readonly IProtocolLinkedProgressiveAdapter _protocolLinkedProgressiveAdapter;
        private readonly IGameRoundDetailsDisplayProvider _gameRoundDetailsDisplayProvider;
        private readonly IReelController _reelController;
        private readonly ISystemDisableManager _disableManager;
        private readonly DetailedGameMetersViewModel _detailedGameMetersViewModel;
        private bool _resetScrollToTop;
        private bool _replayPauseEnabled;
        private bool _replayPauseActive;
        private ObservableCollection<string> _selectedGameRoundTextList;
        private ObservableCollection<FilterObject> _filterGameNames = new();
        private ObservableCollection<FilterObject> _filterStatuses = new();
        private DateTime? _filterStartDate;
        private DateTime? _filterEndDate;
        private DateTime? _filterSelectedDate;
        private bool? _selectAllGameNamesIsChecked;
        private bool? _selectAllStatusesIsChecked;
        private bool _selectingAll;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameHistoryViewModel" /> class.
        /// </summary>
        public GameHistoryViewModel()
        {
            ShowSequenceNumber = true;

            ShowGameRoundInfo = GetConfigSetting(OperatorMenuSetting.PrintGameRoundInfo, false);
            _eventsPerPage = ShowGameRoundInfo ? 1 : 2;
            GameHistory = new ObservableCollection<GameRoundHistoryItem>();
            FilteredGameHistory = new ObservableCollection<GameRoundHistoryItem>();

            _meterFreeGamesIndependently = PropertiesManager.GetValue(
                GamingConstants.MeterFreeGamesIndependently,
                false);

            ShowGameInfoButtons = GetConfigSetting(OperatorMenuSetting.ShowGameInfoButtons, false);
            ShowProgressiveDetails = GetConfigSetting(OperatorMenuSetting.ShowProgressiveDetails, false);

            if (!InDesigner)
            {
                var container = ServiceManager.GetInstance().GetService<IContainerService>();

                _lobbyStateManager = container.Container.GetInstance<ILobbyStateManager>();
                _gameHistoryProvider = container.Container.GetInstance<IGameHistory>();
                _gameDiagnostics = container.Container.GetInstance<IGameDiagnostics>();
                _gameRecovery = container.Container.GetInstance<IGameRecovery>();
                _gamePlayState = container.Container.GetInstance<IGamePlayState>();
                _bank = container.Container.GetInstance<IBank>();
                _progressiveProvider = container.Container.GetInstance<IProgressiveLevelProvider>();
                _propertiesManager = container.Container.GetInstance<IPropertiesManager>();
                _centralProvider = container.Container.GetInstance<ICentralProvider>();
                _protocolLinkedProgressiveAdapter =
                    container.Container.GetInstance<IProtocolLinkedProgressiveAdapter>();
                _disableManager = container.Container.GetInstance<ISystemDisableManager>();

                _gameRoundDetailsDisplayProvider = ServiceManager.GetInstance().TryGetService<IGameRoundDetailsDisplayProvider>();
                _reelController = ServiceManager.GetInstance().TryGetService<IReelController>();

                _detailedGameMetersViewModel = container.Container.GetInstance<DetailedGameMetersViewModel>();
                // Hide the sequence number if we're going to make free games look like independent games (ALC Only)
                ShowSequenceNumber = !_meterFreeGamesIndependently;

                _gameLogs = _gameHistoryProvider.GetGameHistory().ToArray();
            }

            _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();

            ReplayCommand = new ActionCommand<object>(ReplayPressed);
            ShowGameMetersCommand = new ActionCommand<object>(ShowGameMeters);
            ShowGameTransactionsCommand = new ActionCommand<object>(ShowGameTransactions);
            ShowGameProgressiveWinCommand = new ActionCommand<object>(ShowGameProgressiveWin);
            ShowGameEventLogsCommand = new ActionCommand<object>(ShowGameEventLogs);
            ShowGameDetailsCommand = new ActionCommand<object>(ShowGameDetails);
            ShowProgressiveDetailsCommand = new ActionCommand<object>(ShowProgressiveDetailsPopup);

            ReplayPauseActive = PropertiesManager.GetValue(GamingConstants.ReplayPauseActive, true);
            ReplayPauseEnabled = PropertiesManager.GetValue(GamingConstants.ReplayPauseEnable, true);

            ShowGameDetailsButton = _gameRoundDetailsDisplayProvider != null;

            ShowGameEventLogsButton =
                PropertiesManager.GetValue(GamingConstants.KeepGameRoundEvents, true) &&
                ShowGameInfoButtons;
        }

        public ICommand ReplayCommand { get; }

        public ICommand ShowGameMetersCommand { get; }

        public ICommand ShowGameTransactionsCommand { get; }

        public ICommand ShowGameProgressiveWinCommand { get; }

        public ICommand ShowGameEventLogsCommand { get; }

        public ICommand ShowGameDetailsCommand { get; }

        public ICommand ShowProgressiveDetailsCommand { get; }

        public bool ShowSequenceNumber { get; }

        public ObservableCollection<GameRoundHistoryItem> GameHistory
        {
            get => _gameHistory;
            set
            {
                _gameHistory = value;
                RaisePropertyChanged(nameof(GameHistory));
                RaisePropertyChanged(nameof(ReplayButtonEnabled));
                RaisePropertyChanged(nameof(EnablePrintCurrentPageButton));
                RaisePropertyChanged(nameof(MainPrintButtonEnabled));
            }
        }

        public ObservableCollection<GameRoundHistoryItem> FilteredGameHistory { get; }

        public bool? SelectAllGameNamesIsChecked
        {
            get => _selectAllGameNamesIsChecked;
            set
            {
                if (SetProperty(ref _selectAllGameNamesIsChecked, value))
                {
                    _selectingAll = true;
                    SelectAllFilters(FilterGameNames, _selectAllGameNamesIsChecked);
                    _selectingAll = false;
                }
            }
        }

        public ObservableCollection<FilterObject> FilterGameNames
        {
            get => _filterGameNames;
            set => SetProperty(ref _filterGameNames, value);
        }

        public bool? SelectAllStatusesIsChecked
        {
            get => _selectAllStatusesIsChecked;
            set
            {
                if (SetProperty(ref _selectAllStatusesIsChecked, value))
                {
                    _selectingAll = true;
                    SelectAllFilters(FilterStatuses, _selectAllStatusesIsChecked);
                    _selectingAll = false;
                }
            }
        }

        public ObservableCollection<FilterObject> FilterStatuses
        {
            get => _filterStatuses;
            set => SetProperty(ref _filterStatuses, value);
        }

        public DateTime? FilterStartDate
        {
            get => _filterStartDate;
            set => SetProperty(ref _filterStartDate, value);
        }

        public DateTime? FilterEndDate
        {
            get => _filterEndDate;
            set => SetProperty(ref _filterEndDate, value);
        }

        public DateTime? FilterSelectedDate
        {
            get => _filterSelectedDate;
            set
            {
                if (!SetProperty(ref _filterSelectedDate, value))
                {
                    return;
                }

                FilterGameHistory();
            }
        }

        public decimal PendingCurrencyIn => _gameHistoryProvider.PendingCurrencyIn.MillicentsToDollars();

        public bool ResetScrollToTop
        {
            get => _resetScrollToTop;
            set
            {
                _resetScrollToTop = value;
                RaisePropertyChanged(nameof(ResetScrollToTop));
            }
        }

        public bool ReplayPauseEnabled
        {
            get => _replayPauseEnabled;
            set
            {
                _replayPauseEnabled = value;
                RaisePropertyChanged(nameof(ReplayPauseEnabled));
            }
        }

        public bool ReplayPauseActive
        {
            get => _replayPauseActive;
            set
            {
                _replayPauseActive = value;
                RaisePropertyChanged(nameof(ReplayPauseActive));
                _propertiesManager.SetProperty(GamingConstants.ReplayPauseActive, ReplayPauseActive);
            }
        }

        public ObservableCollection<string> SelectedGameRoundTextList
        {
            get => _selectedGameRoundTextList;
            set => SetProperty(ref _selectedGameRoundTextList, value);
        }

        public GameRoundHistoryItem SelectedGameItem
        {
            get => _selectedGameItem;

            set
            {
                if (_selectedGameItem != value)
                {
                    ResetScrollToTop = true;
                    _selectedGameItem = value;

                    var outComeText =
                        _centralProvider.Transactions.ElementAtOrDefault(_selectedGameItem?.ReplayIndex ?? -1)
                            ?.Descriptions.OfType<TextOutcomeDescription>()
                            .Select(x => x.GameRoundInfo)
                            .Where(x => !string.IsNullOrEmpty(x)) ?? Enumerable.Empty<string>();
                    var gameRoundDescription = _selectedGameItem?.GameRoundDescriptionText +
                                               NewLineChar +
                                               string.Join($"{NewLineChar}", outComeText);
                    var gameRoundDescriptionTextItems = gameRoundDescription.Split(NewLineChar)
                        .Where(x => !(string.IsNullOrWhiteSpace(x) ||
                                      x.StartsWith("\0", StringComparison.InvariantCulture)))
                        .SelectMany(SplitGameRoundInfo);
                    SelectedGameRoundTextList = new ObservableCollection<string>(gameRoundDescriptionTextItems);

                    RaisePropertyChanged(nameof(ReplayButtonEnabled));
                    RaisePropertyChanged(nameof(SelectedGameItem));
                    RaisePropertyChanged(nameof(EnablePrintSelectedButton));
                    RaisePropertyChanged(nameof(IsGameRoundComplete));
                    RaisePropertyChanged(nameof(IsHistoryItemSelected));
                    RaisePropertyChanged(nameof(GameProgressiveWinButtonEnabled));
                    RaisePropertyChanged(nameof(IsMeteredGameSelected));
                    RaisePropertyChanged(nameof(SelectedGameHasProgressiveDetails));
                    UpdateStatusText();
                    ResetScrollToTop = false;
                }
            }
        }

        /// <summary>
        /// Is a game currently being replayed?
        /// </summary>
        public bool IsReplaying
        {
            get => _isReplaying;
            set
            {
                _isReplaying = value;
                RaisePropertyChanged(nameof(IsReplaying), nameof(ReplayButtonEnabled));
            }
        }

        public bool ReplayButtonEnabled => SelectedGameItem != null &&
                                           _selectedGameItem.CanReplay &&
                                           !IsReplaying &&
                                          !SelectedGameItem.EndTime.Equals(DateTime.MinValue) &&
                                          AllowReplayDuringGame &&
                                          !_gameHistoryProvider.IsRecoveryNeeded &&
                                          !_propertiesManager.GetValue(
                                              GamingConstants.AdditionalInfoGameInProgress,
                                              false) &&
                                          IsReelControllerAvailable();

        public bool IsGameRoundComplete => SelectedGameItem?.Status ==
                                           Localizer.For(CultureFor.Operator)
                                               .GetString(ResourceKeys.GameHistoryStatusComplete);

        public bool ShowGameRoundInfo { get; }

        public bool EnablePrintSelectedButton => SelectedGameItem != null &&
                                                 !SelectedGameItem.EndTime.Equals(DateTime.MinValue) &&
                                                 PrinterButtonsEnabled;

        public bool EnablePrintLast15Button => GameHistory != null &&
                                               GameHistory.Any(a => !a.EndTime.Equals(DateTime.MinValue)) &&
                                               PrinterButtonsEnabled;

        public bool EnablePrintCurrentPageButton => GameHistory != null &&
                                                    GameHistory.Any(a => !a.EndTime.Equals(DateTime.MinValue)) &&
                                                    PrinterButtonsEnabled;

        public bool GameProgressiveWinButtonEnabled =>
            SelectedGameItem != null && !SelectedGameItem.EndTime.Equals(DateTime.MinValue);

        public bool ShowGameInfoButtons { get; }

        public bool IsMeteredGameSelected
        {
            get
            {
                if (SelectedGameItem == null || _gameLogs == null)
                {
                    return false;
                }

                var gameHistoryItem = _gameLogs
                    .FirstOrDefault(g => g.LogSequence == SelectedGameItem.LogSequence);

                return IsGameRoundComplete &&
                       gameHistoryItem?.MeterSnapshots != null &&
                       gameHistoryItem.MeterSnapshots.Any();
            }
        }

        public bool ShowGameDetailsButton { get; }

        public bool ShowGameEventLogsButton { get; }

        public bool ShowProgressiveDetails { get; }

        public bool SelectedGameHasProgressiveDetails => IsGameRoundComplete && (SelectedGameItem?.EndJackpots?.Any() ?? false);

        public bool IsHistoryItemSelected => SelectedGameItem != null;

        public override bool DataEmpty => GameHistory == null || !GameHistory.Any();

        public override bool MainPrintButtonEnabled =>
            base.MainPrintButtonEnabled && GameHistory.Any(o => !o.EndTime.Equals(DateTime.MinValue));

        protected override void InitializeData()
        {
            //RefreshGameHistory();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            if (!_gameDiagnostics?.IsActive ?? false)
            {
                IsReplaying = false;
            }

            EventBus.Publish(new GameHistoryPageLoadedEvent());

            EventBus.Subscribe<TransactionCompletedEvent>(this, HandleUpdate);
            EventBus.Subscribe<BankBalanceChangedEvent>(this, HandleUpdate);
            EventBus.Subscribe<PrintStartedEvent>(this, PrintStatusChanged);
            EventBus.Subscribe<HardwareReelFaultEvent>(this, HandleReelRelatedFault);
            EventBus.Subscribe<HardwareReelFaultClearEvent>(this, HandleReelRelatedFault);
            EventBus.Subscribe<ReelStoppedEvent>(this, HandleReelRelatedFault);
            EventBus.Subscribe<SystemDisableAddedEvent>(this, HandleSystemDisableAddedEvent);
            EventBus.Subscribe<SystemDisableRemovedEvent>(this, HandleSystemDisableRemovedEvent);

            // We don't need to subscribe to completed events because that is handled by the UpdatePrinterButtons override

            SelectedGameItem = null;
            RaisePropertyChanged(nameof(PrintCurrentPageButtonVisible));
            RaisePropertyChanged(nameof(PrintSelectedButtonVisible));
            RaisePropertyChanged(nameof(PrintLast15ButtonVisible));
            RaisePropertyChanged(nameof(ReplayButtonEnabled));
            RaisePropertyChanged(nameof(PendingCurrencyIn));
            RefreshGameHistory();
        }

        protected override void OnUnloaded()
        {
            ClearFilterList(FilterGameNames);
            ClearFilterList(FilterStatuses);
            base.OnUnloaded();
        }

        protected override void UpdatePrinterButtons()
        {
            RaisePropertyChanged(nameof(EnablePrintCurrentPageButton));
            RaisePropertyChanged(nameof(EnablePrintSelectedButton));
            RaisePropertyChanged(nameof(EnablePrintLast15Button));
            RaisePropertyChanged(nameof(ReplayButtonEnabled));
        }

        protected override void UpdateStatusText()
        {
            var text = string.Empty;
            if (!ReplayButtonEnabled)
            {
                if (_gamePlayState.InGameRound || _propertiesManager.GetValue(
                    GamingConstants.AdditionalInfoGameInProgress,
                    false))
                {
                    text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EndGameRoundBeforeReplay);
                }
                else if (!AllowReplayDuringGame && PropertiesManager.GetValue(GamingConstants.IsGameRunning, false))
                {
                    text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReplayDisabledInGameText);
                }
                else if (_gameHistoryProvider.IsRecoveryNeeded)
                {
                    text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReplayDisabledInRecoveryText);
                }
                else if (!IsReelControllerAvailable())
                {
                    text = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ReplayDisabledReelFaultText);
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(text));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        private bool AllowReplayDuringGame => _lobbyStateManager.AllowGameInCharge ||
                                              (!PropertiesManager.GetValue(GamingConstants.IsGameRunning, false) &&
                                               !_gameHistoryProvider.IsRecoveryNeeded);

        private static IEnumerable<string> SplitGameRoundInfo(string info)
        {
            const string splitLinePadding = "    ";
            var isSplit = false;
            while (info.Length > MaxGameRoundInfoTextLength)
            {
                var endPos = info.LastIndexOf(' ', MaxGameRoundInfoTextLength);
                if (endPos == -1)
                {
                    endPos = MaxGameRoundInfoTextLength;
                }
                else
                {
                    endPos++;
                }

                var infoLine = info.Substring(0, endPos);
                info = info.Substring(endPos);
                yield return isSplit ? splitLinePadding + infoLine : infoLine;
                isSplit = true;
            }

            yield return isSplit ? splitLinePadding + info : info;
        }

        private void ReplayPressed(object obj)
        {
            if (SelectedGameItem != null)
            {
                // Disable during replay.
                IsReplaying = true;

                if (_gameRecovery.IsRecovering)
                {
                    _gameRecovery.AbortRecovery();
                }

                var log = _gameHistoryProvider.GetByIndex(SelectedGameItem.ReplayIndex);

                PreventOperatorMenuExit();

                _gameDiagnostics.Start(
                    SelectedGameItem.GameId,
                    SelectedGameItem.DenomId,
                    SelectedGameItem.RefNoText,
                    new ReplayContext(log, SelectedGameItem.GameIndex),
                    true);
            }
        }

        private void RefreshGameHistory()
        {
            var gameHistoryTemp = !_meterFreeGamesIndependently
                ? GetHistory()
                : GetExpandedHistory();

            var gameHistory = new List<GameRoundHistoryItem>();

            // Base Game Index is -1 when metering Free Game Independently, 0 when not.  This is for Replay.
            var gameHistoryBase = gameHistoryTemp.Where(o => o.GameIndex <= 0 && !o.IsTransactionItem)
                .OrderByDescending(o => o.LogSequence);
            foreach (var baseGame in gameHistoryBase)
            {
                var historyByLogId = gameHistoryTemp.Where(o => o.LogSequence == baseGame.LogSequence).ToList();

                var gamesByLogId = historyByLogId.Where(o => !o.IsTransactionItem).OrderByDescending(o => o.GameIndex);

                foreach (var game in gamesByLogId)
                {
                    var gameIndex = _meterFreeGamesIndependently ? game.GameIndex : 0;
                    var postGameTransactions = historyByLogId
                        .Where(o => o.IsTransactionItem && o.GameIndex == gameIndex)
                        .OrderByDescending(o => o.TransactionId).ToList();

                    if (postGameTransactions.Any())
                    {
                        gameHistory.AddRange(postGameTransactions);
                    }

                    gameHistory.Add(game);
                }

                var preGameTransactions = historyByLogId
                    .Where(o => o.IsTransactionItem && o.GameIndex == -1)
                    .OrderByDescending(o => o.TransactionId).ToList();

                if (preGameTransactions.Any())
                {
                    gameHistory.AddRange(preGameTransactions);
                }
            }

            // go through the list and compute start & end values for Transaction Items
            GameRoundHistoryItem lastItem = null;

            foreach (var item in gameHistory)
            {
                if (item.IsTransactionItem)
                {
                    item.EndCredits = lastItem?.StartCredits
                                      ?? (_bank.QueryBalance() - _gameHistoryProvider.PendingCurrencyIn)
                                      .MillicentsToDollars();
                    item.StartCredits = item.EndCredits.Value + (item.AmountOut ?? -item.AmountIn ?? 0);
                }

                lastItem = item;
            }

            GameHistory = new ObservableCollection<GameRoundHistoryItem>(gameHistory);

            UpdateFilter(FilterGameNames, gameHistory.Select(g => g.GameName).Distinct());
            UpdateFilter(FilterStatuses, gameHistory.Select(g => g.Status).Distinct());
            FilterSelectedDate = null;
            FilterEndDate = gameHistory.FirstOrDefault()?.StartTime;
            FilterStartDate = gameHistory.LastOrDefault()?.StartTime;

            FilterGameHistory();

            UpdatePrinterButtons();
        }

        private void FilterGameHistory()
        {
            if (!_selectingAll)
            {
                // Set the states of the Select All checkboxes
                SelectAllGameNamesIsChecked = GetSelectAllFiltersState(FilterGameNames);
                SelectAllStatusesIsChecked = GetSelectAllFiltersState(FilterStatuses);
            }

            FilteredGameHistory.Clear();
            foreach (var item in GameHistory)
            {
                var filterName = FilterGameNames.FirstOrDefault(filter => filter.Name == item.GameName);
                var filterStatus = FilterStatuses.FirstOrDefault(filter => filter.Name == item.Status);
                if (filterName is { IsChecked: true } && filterStatus is { IsChecked: true } &&
                    (FilterSelectedDate == null || item.StartTime.Date == FilterSelectedDate?.Date))
                {
                    FilteredGameHistory.Add(item);
                }
            }
        }

        private bool? GetSelectAllFiltersState(ICollection<FilterObject> filterList)
        {
            if (filterList.All(f => f.IsChecked))
            {
                return true;
            }

            if (filterList.All(f => !f.IsChecked))
            {
                return false;
            }

            return null;
        }

        private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(FilterObject.IsChecked))
            {
                FilterGameHistory();
            }
        }

        private void ClearFilterList(ICollection<FilterObject> filterList)
        {
            foreach (var filter in filterList)
            {
                filter.PropertyChanged -= Filter_PropertyChanged;
            }
            filterList.Clear();
        }

        private void UpdateFilter(ICollection<FilterObject> filterList, IEnumerable<string> distinctFilters)
        {
            ClearFilterList(filterList);

            foreach (var status in distinctFilters)
            {
                var filter = new FilterObject(status);
                filter.PropertyChanged += Filter_PropertyChanged;
                filterList.Add(filter);
            }
        }

        private void SelectAllFilters(ICollection<FilterObject> filterList, bool? isChecked)
        {
            if (isChecked.HasValue)
            {
                foreach (var filterObject in filterList)
                {
                    filterObject.IsChecked = isChecked.Value;
                }
            }
            FilterGameHistory();
        }

        private List<GameRoundHistoryItem> GetHistory()
        {
            // Need all games (active and uninstalled)
            var games = PropertiesManager.GetValues<IGameDetail>(GamingConstants.AllGames).ToList();
            var history = _gameHistoryProvider.GetGameHistory().OrderByDescending(h => h.LogSequence).ToList();
            var maxCount = Math.Min(_gameHistoryProvider.MaxEntries, history.Count);

            var index = 0;

            var rounds = new List<GameRoundHistoryItem>();

            foreach (var gameHistory in history.Take(maxCount))
            {
                var game = games.FirstOrDefault(g => g.Id == gameHistory.GameId);
                var gameExists = File.Exists(game?.GameDll);

                var round = new GameRoundHistoryItem
                {
                    GameId = gameHistory.GameId,
                    StartTime = gameHistory.StartDateTime,
                    StartCredits = gameHistory.StartCredits.MillicentsToDollars(),
                    EndTime = gameHistory.EndDateTime,
                    EndCredits = gameHistory.PlayState != PlayState.Idle
                        ? null
                        : gameHistory.EndCredits.MillicentsToDollars(),
                    EndJackpot = BuildJackpotString(gameHistory.JackpotSnapshotEnd?.ToList()),
                    EndJackpots = gameHistory.JackpotSnapshotEnd,
                    DenomId = gameHistory.DenomId,
                    Denom = gameHistory.DenomId.MillicentsToDollars(),
                    CreditsWon = gameHistory.PlayState != PlayState.Idle
                        ? null
                        : gameHistory.TotalWon.CentsToDollars(),
                    CreditsWagered = gameHistory.FinalWager.CentsToDollars(),
                    AmountIn = null,
                    AmountOut = null,
                    LogSequence = gameHistory.LogSequence,
                    GameState = gameHistory.PlayState,
                    GameRoundDescriptionText = gameHistory.GameRoundDescriptions,
                    CanReplay = gameHistory.PlayState == PlayState.Idle && gameHistory.FinalWager != 0 && gameExists,
                    Status = GetStatusText(gameHistory),
                    RefNoText = $"{maxCount - index} {GameHistoryTicket.SequenceDelimiter} {maxCount}",
                    ReplayIndex = gameHistory.StorageIndex
                };

                if (game != null)
                {
                    round.GameName = $"{game.ThemeName} ({game.VariationId})";
                    round.GameVersion = game.Version;
                }

                FillTransactionData(ref round, gameHistory.Transactions);

                rounds.Add(round);

                index++;
            }

            return rounds;

            string GetStatusText(IGameHistoryLog data)
            {
                switch (data.PlayState)
                {
                    case PlayState.FatalError:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusFatalError);
                    case PlayState.Idle:
                        return GetIdleStatusText(data);
                    default:
                        return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusInProgress);
                }
            }
        }

        private List<GameRoundHistoryItem> GetExpandedHistory()
        {
            // Need all games (active and uninstalled)
            var games = PropertiesManager.GetValues<IGameDetail>(GamingConstants.AllGames).ToList();
            var history = _gameHistoryProvider.GetGameHistory().OrderByDescending(h => h.LogSequence).ToList();
            var freeGamesCount = history.Sum(h => h.FreeGames.Count());
            var historyCount = history.Count + freeGamesCount;
            var maxCount = Math.Min(_gameHistoryProvider.MaxEntries, historyCount);

            if (maxCount < historyCount)
            {
                // find the actual max count we will use.  Always end with a base game
                var count = 0;
                for (var i = 0; i < history.Count && count < maxCount; i++)
                {
                    count += history[i].FreeGames.Count() + 1;
                }

                maxCount = count;
            }

            var rounds = new List<GameRoundHistoryItem>();

            var index = 0;
            foreach (var gameHistory in history)
            {
                var game = games.FirstOrDefault(g => g.Id == gameHistory.GameId);
                var gameExists = File.Exists(game?.GameDll);

                var freeGames = gameHistory.FreeGames.Reverse().ToList();
                DateTime endTime;

                var gameRoundDescriptions = gameHistory.GameRoundDescriptions.Split(
                    new[] { GameDescriptionDelimiter },
                    StringSplitOptions.RemoveEmptyEntries);

                var gameIndex = freeGames.Count;
                foreach (var freeGame in freeGames)
                {
                    endTime = gameIndex == freeGames.Count ? gameHistory.EndDateTime : freeGame.EndDateTime;

                    var freeGameRound = new GameRoundHistoryItem
                    {
                        GameId = gameHistory.GameId,
                        GameName = game?.ThemeName,
                        GameVersion = game?.Version,
                        StartTime = freeGame.StartDateTime,
                        StartCredits = freeGame.StartCredits.MillicentsToDollars(),
                        EndTime = endTime,
                        EndCredits = freeGame.Result == GameResult.None
                            ? null
                            : freeGame.EndCredits.MillicentsToDollars(),
                        EndJackpot = BuildJackpotString(gameHistory.JackpotSnapshotEnd?.ToList()),
                        EndJackpots = gameHistory.JackpotSnapshotEnd,
                        DenomId = gameHistory.DenomId,
                        Denom = gameHistory.DenomId.MillicentsToDollars(),
                        CreditsWon =
                            freeGame.Result == GameResult.None ? null : freeGame.FinalWin.CentsToDollars(),
                        CreditsWagered = 0,
                        AmountIn = null,
                        AmountOut = null,
                        LogSequence = gameHistory.LogSequence,
                        GameState = gameHistory.PlayState,
                        GameRoundDescriptionText = gameRoundDescriptions.ElementAtOrDefault(gameIndex),
                        CanReplay = freeGame.Result != GameResult.None && gameExists,
                        Status =
                            endTime != DateTime.MinValue
                                ? BuildStatusText(gameHistory, freeGame)
                                : Localizer.For(CultureFor.Operator)
                                    .GetString(ResourceKeys.GameHistoryStatusInProgress),
                        RefNoText = $"{maxCount - index} {GameHistoryTicket.SequenceDelimiter} {maxCount}",
                        ReplayIndex = gameHistory.StorageIndex,
                        GameIndex = gameIndex
                    };

                    if (gameHistory.PlayState != PlayState.FatalError)
                    {
                        rounds.Add(freeGameRound);
                        index++;
                    }

                    gameIndex--;
                    if (index == maxCount)
                    {
                        break;
                    }
                }

                if (index == maxCount)
                {
                    break;
                }

                if (gameHistory.PlayState == PlayState.FatalError)
                {
                    maxCount -= freeGames.Count;
                }

                if (gameHistory.LastCommitIndex == -1)
                {
                    endTime = DateTime.MinValue;
                }
                else
                {
                    endTime = freeGames.Count == 0
                        ? gameHistory.EndDateTime == DateTime.MinValue ? DateTime.UtcNow : gameHistory.EndDateTime
                        : freeGames.Last().StartDateTime;
                }

                var round = new GameRoundHistoryItem
                {
                    GameId = gameHistory.GameId,
                    GameName = game?.ThemeName,
                    GameVersion = game?.Version,
                    DenomId = gameHistory.DenomId,
                    Denom = gameHistory.DenomId.MillicentsToDollars(),
                    StartTime = gameHistory.StartDateTime,
                    EndTime = endTime,
                    LogSequence = gameHistory.LogSequence,
                    GameState = gameHistory.PlayState,
                    GameRoundDescriptionText =
                        gameRoundDescriptions.ElementAtOrDefault(0) ?? gameHistory.GameRoundDescriptions,
                    CanReplay = gameHistory.PlayState == PlayState.Idle && gameExists,
                    Status = BuildStatusText(gameHistory, null),
                    RefNoText = $"{maxCount - index} {GameHistoryTicket.SequenceDelimiter} {maxCount}",
                    ReplayIndex = gameHistory.StorageIndex,
                    CreditsWagered = gameHistory.FinalWager.CentsToDollars(),
                    AmountIn = null,
                    AmountOut = null,
                    StartCredits = gameHistory.StartCredits.MillicentsToDollars(),
                    EndCredits = gameHistory.PlayState != PlayState.Idle
                        ? null
                        : gameHistory.EndCredits.MillicentsToDollars(),
                    EndJackpot = BuildJackpotString(gameHistory.JackpotSnapshotEnd?.ToList()),
                    EndJackpots = gameHistory.JackpotSnapshotEnd,
                    GameIndex = 0
                };

                if (IsBaseGameCommitted(gameHistory))
                {
                    round.CreditsWon = gameHistory.UncommittedWin.CentsToDollars();
                }
                else
                {
                    var freeGamesTotalWon = freeGames.Sum(f => f.FinalWin);
                    round.CreditsWon = gameHistory.PlayState != PlayState.Idle
                        ? null
                        : (gameHistory.TotalWon - freeGamesTotalWon).CentsToDollars();
                }

                FillTransactionData(ref round, gameHistory.Transactions);
                rounds.Add(round);

                index++;
                if (index == maxCount)
                {
                    break;
                }
            }

            return rounds;
        }

        private void FillTransactionData(
            ref GameRoundHistoryItem round,
            IEnumerable<TransactionInfo> transactions)
        {
            if (transactions == null)
            {
                return;
            }

            foreach (var transaction in transactions)
            {
                if (transaction.TransactionType == typeof(BillTransaction) ||
                    transaction.TransactionType == typeof(VoucherInTransaction) ||
                    transaction.TransactionType == typeof(WatOnTransaction) ||
                    transaction.TransactionType == typeof(KeyedOnCreditsTransaction))
                {
                    if (round.AmountIn == null)
                    {
                        round.AmountIn = 0;
                    }

                    round.AmountIn += transaction.Amount.MillicentsToDollars();
                }
                else if (transaction.TransactionType == typeof(VoucherOutTransaction) ||
                         transaction.TransactionType == typeof(WatTransaction) ||
                         transaction.TransactionType == typeof(KeyedOffCreditsTransaction) ||
                         transaction.TransactionType == typeof(HardMeterOutTransaction) ||
                         transaction.TransactionType == typeof(ResidualCreditsTransaction))
                {
                    if (round.AmountOut == null)
                    {
                        round.AmountOut = 0;
                    }

                    round.AmountOut += transaction.Amount.MillicentsToDollars();
                }
                else if (transaction.TransactionType == typeof(HandpayTransaction))
                {
                    if (round.AmountOut == null)
                    {
                        round.AmountOut = 0;
                    }

                    switch (transaction.KeyOffType)
                    {
                        case KeyOffType.LocalHandpay:
                        case KeyOffType.RemoteHandpay:
                        case KeyOffType.RemoteVoucher:
                        case KeyOffType.LocalVoucher:
                            round.AmountOut += transaction.Amount.MillicentsToDollars();
                            break;
                        case KeyOffType.RemoteCredit:
                        case KeyOffType.LocalCredit:
                            break;
                    }
                }
            }

            if (round.AmountIn == 0)
            {
                round.AmountIn = null;
            }

            if (round.AmountOut == 0)
            {
                round.AmountOut = null;
            }
        }

        private string BuildJackpotString(IList<Jackpot> jackpots)
        {
            if (jackpots == null || !jackpots.Any())
            {
                return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NotAvailable);
            }

            return string.Join(" ", jackpots.Select(j => j.Value.MillicentsToDollars().FormattedCurrencyStringForOperator()));
        }

        private bool IsBaseGameCommitted(IGameHistoryLog gameHistory)
        {
            // LastCommitIndex will be -1 if we're still in the base game.  Once we commit the win for the base game this will be 0 or greater
            return gameHistory.PlayState == PlayState.PrimaryGameStarted && gameHistory.LastCommitIndex >= 0
                                                                         && (gameHistory.FreeGames?.Count() ?? 0) >
                                                                         0; //primary game isn't fully "completed" until first bonus game starts.
        }

        private string BuildStatusText(IGameHistoryLog data, IFreeGameInfo freeGame)
        {
            switch (data.PlayState)
            {
                case PlayState.FatalError:
                    return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusFatalError);
                case PlayState.Idle:
                    return GetIdleStatusText(data);
                default:
                    if (freeGame == null)
                    {
                        return IsBaseGameCommitted(data)
                            ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusComplete)
                            : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusInProgress);
                    }

                    return freeGame.Result == GameResult.None
                        ? Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusInProgress)
                        : Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusComplete);
            }
        }

        private static string GetIdleStatusText(IGameHistoryLog data)
        {
            if (data.Result == GameResult.Failed)
            {
                return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusCompleteNoOutcome);
            }

            return Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameHistoryStatusComplete);
        }

        private List<Ticket> GetLogTickets(OperatorMenuPrintData dataType)
        {
            var logs = GameHistory.ToList();

            if (dataType == OperatorMenuPrintData.Last15)
            {
                logs = logs.Where(x => !x.IsTransactionItem).ToList();
            }

            logs = GetItemsToPrint(logs, dataType).ToList();

            if (logs.Count > 0 && logs[0].EndTime.Equals(DateTime.MinValue))
            {
                // remove the first game if it is in progress
                logs.RemoveAt(0);
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IGameHistoryTicketCreator>();
            if (ticketCreator == null)
            {
                Logger.Info("Couldn't find ticket creator");
                return null;
            }

            var tickets = new List<Ticket>();

            var printNumberOfPages = (int)Math.Ceiling((double)logs.Count / _eventsPerPage);
            for (int page = 0; page < printNumberOfPages; page++)
            {
                var singlePageLogs = logs.Skip(page * _eventsPerPage).Take(_eventsPerPage).ToList();

                foreach (var item in singlePageLogs)
                {
                    if (!item.EndTime.Equals(DateTime.MinValue) || item.IsTransactionItem)
                    {
                        if (ticketCreator.MultiPage(item))
                        {
                            tickets.AddRange(ticketCreator.CreateMultiPage(item));
                        }
                        else
                        {
                            var ticket = ticketCreator.Create(1, singlePageLogs);
                            tickets.Add(ticket);
                            break;
                        }
                    }
                }
            }

            Logger.Debug($"Printing {tickets.Count} game history tickets");

            return tickets;
        }

        private List<Ticket> GetSelectedItemTicket()
        {
            if (SelectedGameItem == null ||
                SelectedGameItem.EndTime.Equals(DateTime.MinValue))
            {
                return null;
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IGameHistoryTicketCreator>();

            return ticketCreator?.MultiPage(SelectedGameItem) ?? false
                ? ticketCreator.CreateMultiPage(SelectedGameItem)
                : TicketToList(ticketCreator?.Create(1, new Collection<GameRoundHistoryItem> { SelectedGameItem }));
        }

        private void PrintStatusChanged(IEvent printEvent)
        {
            RaisePropertyChanged(nameof(ReplayButtonEnabled));
        }

        private void HandleUpdate(IEvent theEvent)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    RefreshGameHistory();
                    RaisePropertyChanged(nameof(PendingCurrencyIn));
                });
        }

        private void HandleReelRelatedFault(IEvent theEvent)
        {
            UpdateStatusText();
            RaisePropertyChanged(nameof(ReplayButtonEnabled));
        }

        private void HandleSystemDisableAddedEvent(SystemDisableAddedEvent theEvent)
        {
            if (theEvent.DisableId == ApplicationConstants.ReelControllerDisconnectedGuid)
            {
                UpdateStatusText();
                RaisePropertyChanged(nameof(ReplayButtonEnabled));
            }
        }

        private void HandleSystemDisableRemovedEvent(SystemDisableRemovedEvent theEvent)
        {
            if (theEvent.DisableId == ApplicationConstants.ReelControllerDisconnectedGuid)
            {
                UpdateStatusText();
                RaisePropertyChanged(nameof(ReplayButtonEnabled));
            }
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    RefreshGameHistory();
                    RaisePropertyChanged(nameof(PendingCurrencyIn));
                });

            base.OnOperatorCultureChanged(evt);
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.Main:
                case OperatorMenuPrintData.CurrentPage:
                case OperatorMenuPrintData.Last15:
                    tickets = GetLogTickets(dataType);
                    break;
                case OperatorMenuPrintData.SelectedItem:
                    tickets = GetSelectedItemTicket();
                    break;
            }

            return tickets;
        }

        private void ShowGameMeters(object o)
        {
            if (SelectedGameItem == null)
            {
                return;
            }

            _detailedGameMetersViewModel.Load(SelectedGameItem.LogSequence, false);

            _dialogService.ShowInfoDialog<DetailedGameMetersView>(
                this,
                _detailedGameMetersViewModel,
                $"#{SelectedGameItem.LogSequenceText} {SelectedGameItem.GameName} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameMeters)}");
        }

        private void ShowGameTransactions(object o)
        {
            if (SelectedGameItem == null)
            {
                return;
            }

            _detailedGameMetersViewModel.Load(SelectedGameItem.LogSequence, true);

            _dialogService.ShowInfoDialog<DetailedGameMetersView>(
                this,
                _detailedGameMetersViewModel,
                $"#{SelectedGameItem.LogSequenceText} {SelectedGameItem.GameName} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameTransactions)}");
        }

        private void ShowGameProgressiveWin(object o)
        {
            var history = _gameHistoryProvider.GetGameHistory();
            var gameHistory = history.SingleOrDefault(
                t => t.GameId == SelectedGameItem?.GameId && t.LogSequence == SelectedGameItem?.LogSequence);

            if (gameHistory == null || !(gameHistory.Jackpots?.Any() ?? false))
            {
                ShowPopup(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoProgressiveWinText));
                return;
            }

            var progressiveWins = new ObservableCollection<ProgressiveWinModel>();
            var multiplier = 1.0 / (double)_propertiesManager.GetProperty("CurrencyMultiplier", null);

            foreach (var jackpot in gameHistory.Jackpots)
            {
                var level = _progressiveProvider.GetProgressiveLevels(
                        jackpot.PackName,
                        gameHistory.GameId,
                        gameHistory.DenomId,
                        jackpot.WagerCredits)
                    .FirstOrDefault(l => l.LevelId == jackpot.LevelId);

                var assignedProgressiveType = level?.AssignedProgressiveId?.AssignedProgressiveType ??
                                              AssignableProgressiveType.None;

                IViewableLinkedProgressiveLevel linkedLevel = null;

                if (!string.IsNullOrEmpty(level?.AssignedProgressiveId?.AssignedProgressiveKey) &&
                    assignedProgressiveType == AssignableProgressiveType.Linked)
                {
                    _protocolLinkedProgressiveAdapter?.ViewLinkedProgressiveLevel(
                        level.AssignedProgressiveId?.AssignedProgressiveKey,
                        out linkedLevel);
                }

                var levelId = linkedLevel?.LevelId ?? jackpot.LevelId;

                progressiveWins.Add(
                    new ProgressiveWinModel
                    {
                        WinDateTime = jackpot.HitDateTime,
                        LevelName = level?.LevelName,
                        WinAmount = (multiplier * jackpot.WinAmount).FormattedCurrencyString(),
                        LevelId = levelId,
                        DeviceId = jackpot.DeviceId,
                        TransactionId = jackpot.TransactionId
                    });
            }

            var viewModel = new GameProgressiveWinViewModel(progressiveWins);

            _dialogService.ShowInfoDialog<GameProgressiveWinView>(
                this,
                viewModel,
                $"{SelectedGameItem.GameName} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveWin)}");
        }

        private void ShowGameEventLogs(object o)
        {
            if (SelectedGameItem == null)
            {
                return;
            }

            var viewModel = new GameEventLogsViewModel(SelectedGameItem.LogSequence);
            _dialogService.ShowInfoDialog<GameEventLogsView>(
                this,
                viewModel,
                $"#{SelectedGameItem.LogSequenceText} {SelectedGameItem.GameName} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.EventLog)}");
        }

        private void ShowGameDetails(object o)
        {
            if (SelectedGameItem is null)
            {
                return;
            }

            var log = _gameHistoryProvider.GetByIndex(SelectedGameItem.ReplayIndex);
            var transaction = _centralProvider.Transactions.SingleOrDefault(t => t.AssociatedTransactions.Contains(log.TransactionId));
            if (_gameRoundDetailsDisplayProvider is null || transaction is null)
            {
                return;
            }

            _gameRoundDetailsDisplayProvider.Display(
                this,
                _dialogService,
                $"#{SelectedGameItem.LogSequenceText} {SelectedGameItem.GameName} {Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameRoundDetailsText)}",
                transaction.TransactionId);
        }

        private void ShowProgressiveDetailsPopup(object o)
        {
            if (SelectedGameItem == null)
            {
                return;
            }

            _dialogService.ShowInfoDialog<ProgressiveSummaryView>(
                this,
                new ProgressiveSummaryViewModel(SelectedGameItem),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.ProgressiveSummaryTitle));
        }

        private bool IsReelControllerAvailable()
        {
            // If the cabinet does not have a reel controller then return true
            if (!(bool)_propertiesManager.GetProperty(ApplicationConstants.ReelControllerEnabled, false))
            {
                return true;
            }

            if (_disableManager == null ||
                _reelController == null ||
                _disableManager.CurrentDisableKeys.Contains(ApplicationConstants.ReelControllerDisconnectedGuid) ||
                !_reelController.Enabled ||
                _reelController.ReelControllerFaults != ReelControllerFaults.None ||
                _reelController.LogicalState != ReelControllerState.IdleAtStops)
            {
                return false;
            }

            return true;
        }

        protected override void GameDiagnosticsComplete()
        {
            IsReplaying = false;
        }
    }

    public class ScrollViewerBehavior
    {
        public static bool GetAutoScrollToTop(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToTopProperty);
        }

        public static void SetAutoScrollToTop(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToTopProperty, value);
        }

        public static readonly DependencyProperty AutoScrollToTopProperty =
            DependencyProperty.RegisterAttached("AutoScrollToTop", typeof(bool), typeof(ScrollViewerBehavior), new PropertyMetadata(false, (o, e) =>
            {
                if (!(o is ScrollViewer scrollViewer))
                {
                    return;
                }

                if ((bool)e.NewValue)
                {
                    scrollViewer.ScrollToTop();
                    SetAutoScrollToTop(o, false);
                }
            }));
    }
}
