namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Models;
    using Kernel;
    using Localization.Properties;
    using Models;
    using Monaco.UI.Common.Services;
    using MVVM;
    using MVVM.Command;
    using Newtonsoft.Json;
    using Views.OperatorMenu;

    /// <summary>
    ///     ViewModel for GamePerformanceView
    /// </summary>
    public class GamePerformanceViewModel : OperatorMenuPageViewModelBase
    {
        private const string CachedDataKey = "GamePerformanceViewModelData";
        private readonly IDialogService _dialogService;
        private readonly IGameProvider _gameProvider;
        private readonly ICache _cache;

        private readonly List<Type> _invalidateEvents = new List<Type>
        {
            typeof(GameAddedEvent),
            typeof(GameRemovedEvent),
            typeof(GameEnabledEvent),
            typeof(GameDisabledEvent),
            typeof(GameDenomChangedEvent),
            typeof(GamePlayInitiatedEvent),
            typeof(GameEndedEvent)
        };

        private GamePerformanceViewModelData _gameData;

        private Dictionary<string, Collection<int>> _deselectedGameThemes = new Dictionary<string, Collection<int>>();
        private bool _hideNeverActive;
        private bool _hidePreviouslyActive;
        private string _selectedGameType;
        private ListSortDirection _sortDirection;
        private string _sortMemberPath;
        private bool _showGameRtpAsRange;

        /// <summary>
        ///     Construct a new instance of GamePerformanceViewModel.
        /// </summary>
        public GamePerformanceViewModel()
        {
            if (!InDesigner)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            }

            var container = ServiceManager.GetInstance().GetService<IContainerService>();
            if (container != null)
            {
                _gameProvider = container.Container.GetInstance<IGameProvider>();
            }

            ShowMoreMetersCommand = new ActionCommand<object>(ShowMoreMetersPressed);
            SortingCommand = new ActionCommand<DataGridSortingEventArgs>(SortDataGrid);

            _cache = ServiceManager.GetInstance().GetService<ICache>();
        }

        public ICommand SortingCommand { get; }

        public bool ShowGameRtpAsRange
        {
            get => _showGameRtpAsRange;
            set
            {
                _showGameRtpAsRange = value;
                RaisePropertyChanged(nameof(ShowGameRtpAsRange));
            }
        }

        public ListSortDirection SortDirection
        {
            get => _sortDirection;
            set
            {
                _sortDirection = value;
                PropertiesManager.SetProperty(
                    GamingConstants.OperatorMenuPerformancePageSortDirection,
                    value == ListSortDirection.Ascending);
            }
        }

        public string SortMemberPath
        {
            get => _sortMemberPath;
            set
            {
                _sortMemberPath = value;
                PropertiesManager.SetProperty(GamingConstants.OperatorMenuPerformancePageSortMemberPath, value);
            }
        }

        /// <summary>
        ///     Get or set whether to hide never active game combos
        /// </summary>
        public bool HideNeverActive
        {
            get => _hideNeverActive;
            set
            {
                _hideNeverActive = value;
                PropertiesManager.SetProperty(GamingConstants.OperatorMenuPerformancePageHideNeverActive, value);
                UpdateGamePerformanceItems();
            }
        }

        /// <summary>
        ///     Get or set whether to hide previously active game combos
        /// </summary>
        public bool HidePreviouslyActive
        {
            get => _hidePreviouslyActive;
            set
            {
                _hidePreviouslyActive = value;
                PropertiesManager.SetProperty(GamingConstants.OperatorMenuPerformancePageHidePreviouslyActive, value);
                UpdateGamePerformanceItems();
            }
        }

        /// <summary>
        ///     Get or set Machine Weighted Theoretical Payback
        /// </summary>
        public decimal MachineWeightedPayback
        {
            get => _gameData?.MachineWeightedPayback ?? 0;
            set
            {
                if (_gameData != null)
                {
                    _gameData.MachineWeightedPayback = value;
                    RaisePropertyChanged(nameof(MachineWeightedPayback));
                }
            }
        }

        /// <summary>
        ///     Get or set Machine Actual Operating Payback
        /// </summary>
        public decimal MachineActualPayback
        {
            get => _gameData?.MachineActualPayback ?? 0;
            set
            {
                if (_gameData != null)
                {
                    _gameData.MachineActualPayback = value;
                    RaisePropertyChanged(nameof(MachineActualPayback));
                }
            }
        }

        /// <summary>
        ///     Get or set the list of GamePerformanceData to display
        /// </summary>
        public List<GamePerformanceData> GamePerformanceItems
        {
            get => _gameData?.GamePerformanceItems;
            set
            {
                if (_gameData != null)
                {
                    _gameData.GamePerformanceItems = value;
                    RaisePropertyChanged(nameof(GamePerformanceItems));
                }
            }
        }

        /// <summary>
        ///     Get or set the selected GameType to be used for filtering the lists of GameThemes and GamePerformanceItems
        /// </summary>
        public string SelectedGameType
        {
            get => _selectedGameType;
            set
            {
                _selectedGameType = value;
                PropertiesManager.SetProperty(GamingConstants.OperatorMenuPerformancePageSelectedGameType, value);
                UpdateGameThemes();
                UpdateGamePerformanceItems();
                RaisePropertyChanged(nameof(SelectedGameType));
            }
        }

        /// <summary>
        ///     Get or set the list of all the GameTypes (+ "All")
        /// </summary>
        public ObservableCollection<string> GameTypes
        {
            get => _gameData?.GameTypes;
            set
            {
                if (_gameData != null)
                {
                    _gameData.GameTypes = value;
                    RaisePropertyChanged(nameof(GameTypes));
                }
            }
        }

        /// <summary>
        ///     Get or set the list of GameThemes (filtered by SelectedGameType).  Subscribe to each GameTheme's PropertyChanged
        ///     event
        /// </summary>
        public ObservableCollection<GamePerformanceGameTheme> GameThemes
        {
            get => _gameData?.GameThemes;
            set
            {
                if (_gameData != null)
                {
                    if (_gameData.GameThemes != value)
                    {
                        if (_gameData.GameThemes != null)
                        {
                            foreach (var gameTheme in _gameData.GameThemes)
                            {
                                gameTheme.PropertyChanged -= OnThemeSelectionChanged;
                            }
                        }

                        _gameData.GameThemes = value;

                        foreach (var gameTheme in _gameData.GameThemes)
                        {
                            gameTheme.PropertyChanged += OnThemeSelectionChanged;
                        }

                        RaisePropertyChanged(nameof(GameThemes));
                    }
                }
            }
        }

        /// <summary>
        ///     Gets the ShowMoreMetersCommand
        /// </summary>
        public ICommand ShowMoreMetersCommand { get; }

        private void SortDataGrid(DataGridSortingEventArgs e)
        {
            var column = e.Column;

            var sortDirection = ListSortDirection.Ascending;

            if (SortMemberPath == column.SortMemberPath)
            {
                sortDirection = SortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }

            SortDirection = sortDirection;
            SortMemberPath = column.SortMemberPath;

            column.SortDirection = SortDirection;
            var lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(GamePerformanceItems);

            lcv.CustomSort = new GamePerformanceItemSorter(SortMemberPath, sortDirection);

            e.Handled = true;
        }

        private void SortDataGrid()
        {
            if (SortDirection == ListSortDirection.Ascending)
            {
                GamePerformanceItems = GamePerformanceItems.OrderBy(g => g.ActiveState)
                    .ThenBy(g => typeof(GamePerformanceData).GetProperty(SortMemberPath)?.GetValue(g)).ToList();
            }
            else
            {
                GamePerformanceItems = GamePerformanceItems.OrderBy(g => g.ActiveState)
                    .ThenByDescending(g => typeof(GamePerformanceData).GetProperty(SortMemberPath)?.GetValue(g)).ToList();
            }
        }

        protected override void DisposeInternal()
        {
            if (_gameData?.GameThemes != null)
            {
                foreach (var gameTheme in _gameData.GameThemes)
                {
                    gameTheme.PropertyChanged -= OnThemeSelectionChanged;
                }
            }
        }

        protected override void OnUnloaded()
        {
        }

        protected override void OnLoaded()
        {
            LoadData();

            EventBus.Subscribe<GameEnabledEvent>(this, evt => UpdateGameStatus(evt.GameId, true));
            EventBus.Subscribe<GameDisabledEvent>(this, evt => UpdateGameStatus(evt.GameId, false));
            EventBus.Subscribe<GameAddedEvent>(this, evt => UpdateGameStatus(evt.GameId, true));
            EventBus.Subscribe<GameRemovedEvent>(this, evt => UpdateGameStatus(evt.GameId, false));
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            foreach (var item in GamePerformanceItems)
            {
                item.UpdateCulture();
            }
            base.OnOperatorCultureChanged(evt);
        }

        private void UpdateGameStatus(int gameId, bool enable)
        {
            var game = GamePerformanceItems.FirstOrDefault(g => g.GameId == gameId);
            if (game != null)
            {
                game.IsActive = enable;
                if (!enable)
                {
                    game.PreviousActiveTime += DateTime.UtcNow - game.ActiveDateTime;
                }
            }

            // Re-sort to have enabled at the top
            SortDataGrid();
        }

        /// <summary>
        ///     Define all of the unfiltered data and initial filter/sort state
        /// </summary>
        protected void LoadData()
        {
            LoadProperties();
            var savedSelectedGameType = _selectedGameType;
            _gameData = _cache.Get<GamePerformanceViewModelData>("GamePerformanceViewModelData");
            if (_gameData == null)
            {
                _gameData = new GamePerformanceViewModelData();
                _cache.Add(CachedDataKey, _gameData, _invalidateEvents);

                var meterManager = ServiceManager.GetInstance().GetService<IGameMeterManager>();

                // Initialization logic
                var allGames = _gameProvider.GetGames();

                // Max length of Game Number is used to pad numbers for sorting numeric values alphabetically
                var maxGameNumberLength = 0;
                if (allGames.Any())
                {
                    maxGameNumberLength = allGames.SelectMany(g => g.Denominations, (g, d) => d)
                        .Select(d => d.Id.ToString())
                        .Max(id => id.Length);
                }

                _gameData.AllGamePerformanceItems = new List<GamePerformanceData>(
                    allGames.SelectMany(
                        game => game.Denominations,
                        (g, d) => new GamePerformanceData
                        {
                            GameName = g.ThemeName,
                            GameId = g.Id,
                            GameNumber = d.Id,
                            MaxGameNumberLength = maxGameNumberLength,
                            PaytableId = g.VariationId,
                            Denomination = d.Value.MillicentsToDollars(),
                            ActiveDateTime = d.ActiveDate,
                            PreviousActiveTime = d.PreviousActiveTime,
                            GamesPlayed = meterManager.GetMeter(g.Id, d.Value, GamingMeters.PlayedCount).Lifetime,
                            AmountInMillicents =
                                meterManager.GetMeter(g.Id, d.Value, GamingMeters.WageredAmount).Lifetime,
                            AmountOutMillicents =
                                meterManager.GetMeter(g.Id, d.Value, GamingMeters.TotalPaidAmt).Lifetime,
                            TheoreticalRtp =
                                new Tuple<decimal, decimal>(g.MinimumPaybackPercent, g.MaximumPaybackPercent),
                            GameType = g.GameType,
                            GameSubtype = g.GameSubtype,
                            IsActive = g.Enabled && g.EgmEnabled && g.ActiveDenominations.Contains(d.Value)
                        }));

                var strAll = ResourceKeys.AllGameTypes;
                _gameData.AllGameThemes = new List<GamePerformanceGameTheme>(
                    allGames.Select(
                        g => new GamePerformanceGameTheme
                        {
                            Name = g.ThemeName,
                            Checked = !_deselectedGameThemes.ContainsKey(strAll) || !_deselectedGameThemes[strAll].Contains(g.Id),
                            GameType = g.GameType.ToString(),
                            GameSubtype = g.GameSubtype,
                            GameId = g.Id
                        }
                    )).GroupBy(n => n.Name).Select(th => th.First()).OrderBy(th => th.Name).ToList();

                _gameData.GameTypes =
                    new ObservableCollection<string>(allGames.Select(g => g.GameType.ToString()).Distinct()) { strAll };

                _gameData.GameTypeGameThemes = new Dictionary<string, List<GamePerformanceGameTheme>>();
                foreach (var gameType in _gameData.GameTypes)
                {
                    var tmpGameThemes = new List<GamePerformanceGameTheme>(
                            allGames.Select(
                                g => new GamePerformanceGameTheme
                                {
                                    Name = g.ThemeName,
                                    Checked = !_deselectedGameThemes.ContainsKey(gameType) || !_deselectedGameThemes[gameType].Contains(g.Id),
                                    GameType = g.GameType.ToString(),
                                    GameSubtype = g.GameSubtype,
                                    GameId = g.Id
                                }
                            )).Where(t => t.GameType == gameType).GroupBy(n => n.Name).Select(th => th.First())
                        .OrderBy(th => th.Name).ToList();
                    _gameData.GameTypeGameThemes.Add(gameType, tmpGameThemes);

                    if (!_deselectedGameThemes.ContainsKey(gameType))
                    {
                        _deselectedGameThemes.Add(gameType, new Collection<int>());
                    }
                }

                decimal totalAmountIn = _gameData.AllGamePerformanceItems.Sum(g => g.AmountInMillicents);
                decimal totalAmountOut = _gameData.AllGamePerformanceItems.Sum(g => g.AmountOutMillicents);
                if (totalAmountIn > 0)
                {
                    MachineActualPayback = 100 * totalAmountOut / totalAmountIn;

                    var weightedData = new List<decimal>(
                        allGames.SelectMany(
                            game => game.WagerCategories,
                            (g, w) => w.TheoPaybackPercent * meterManager.GetMeter(
                                g.Id,
                                w.Id,
                                GamingMeters.WagerCategoryWageredAmount).Lifetime
                        ));

                    MachineWeightedPayback = weightedData.Sum() / totalAmountIn;
                }
                else
                {
                    MachineActualPayback = 0;
                    MachineWeightedPayback = 0;
                }
            }

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    RaisePropertyChanged(nameof(GameTypes));
                    SelectedGameType = savedSelectedGameType;
                    UpdateGameThemes();
                    UpdateGamePerformanceItems();

                    RaisePropertyChanged(nameof(HideNeverActive));
                    RaisePropertyChanged(nameof(HidePreviouslyActive));
                    RaisePropertyChanged(nameof(MachineWeightedPayback));
                    RaisePropertyChanged(nameof(MachineActualPayback));
                });
        }

        private void UpdateGameThemes()
        {
            if (_gameData != null)
            {
                GameThemes = IsShowAllGameTypes()
                    ? new ObservableCollection<GamePerformanceGameTheme>(_gameData.AllGameThemes)
                    : new ObservableCollection<GamePerformanceGameTheme>(
                        _gameData.GameTypeGameThemes[_selectedGameType]);
            }
        }

        private void LoadProperties()
        {
            var strAll = ResourceKeys.AllGameTypes;
            _selectedGameType = (string)PropertiesManager.GetProperty(
                GamingConstants.OperatorMenuPerformancePageSelectedGameType,
                strAll);
            if (string.IsNullOrEmpty(_selectedGameType))
            {
                _selectedGameType = strAll;
            }

            _hideNeverActive = (bool)PropertiesManager.GetProperty(
                GamingConstants.OperatorMenuPerformancePageHideNeverActive,
                false);
            _hidePreviouslyActive = (bool)PropertiesManager.GetProperty(
                GamingConstants.OperatorMenuPerformancePageHidePreviouslyActive,
                false);
            _sortDirection =
                (bool)PropertiesManager.GetProperty(GamingConstants.OperatorMenuPerformancePageSortDirection, true)
                    ? ListSortDirection.Ascending
                    : ListSortDirection.Descending;
            _sortMemberPath = (string)PropertiesManager.GetProperty(
                GamingConstants.OperatorMenuPerformancePageSortMemberPath,
                "");
            var jsonText = (string)PropertiesManager.GetProperty(
                GamingConstants.OperatorMenuPerformancePageDeselectedGameThemes,
                string.Empty);
            _deselectedGameThemes = string.IsNullOrEmpty(jsonText) ? new Dictionary<string, Collection<int>>() : JsonConvert.DeserializeObject<Dictionary<string, Collection<int>>>(jsonText);
            ShowGameRtpAsRange = GetGlobalConfigSetting(OperatorMenuSetting.ShowGameRtpAsRange, true);

            if (string.IsNullOrEmpty(_sortMemberPath))
            {
                _sortMemberPath = "GameName";
                _sortDirection = ListSortDirection.Ascending;
            }
        }

        /// <summary>
        ///     Using the current filter/sort settings, set the GamePerformanceItems to display to the operator
        /// </summary>
        private void UpdateGamePerformanceItems()
        {
            if (_gameData != null)
            {
                var perfItems = from item in _gameData.AllGamePerformanceItems
                                join theme in GameThemes
                                    on item.GameName equals theme.Name
                                where theme.Checked
                                select item;

                if (HideNeverActive)
                {
                    perfItems = perfItems.Where(
                        i => i.ActiveState != GamePerformanceData.GamePerformanceActiveState.NeverActive);
                }

                if (HidePreviouslyActive)
                {
                    perfItems = perfItems.Where(
                        i => i.ActiveState != GamePerformanceData.GamePerformanceActiveState.PreviouslyActive);
                }

                if (!IsShowAllGameTypes())
                {
                    perfItems = perfItems.Where(x => x.GameType.ToString() == SelectedGameType);
                }

                var sortProperty = typeof(GamePerformanceData).GetProperty(SortMemberPath);
                if (sortProperty != null)
                {
                    if (SortDirection == ListSortDirection.Ascending)
                    {
                        perfItems = perfItems.OrderBy(x => x.ActiveState).ThenBy(x => sortProperty.GetValue(x))
                            .ThenBy(x => x.GameName).ThenBy(x => x.GameNumber);
                    }
                    else
                    {
                        perfItems = perfItems.OrderBy(x => x.ActiveState).ThenByDescending(x => sortProperty.GetValue(x))
                            .ThenBy(x => x.GameName).ThenBy(x => x.GameNumber);
                    }
                }

                GamePerformanceItems = perfItems.ToList();
            }
        }

        /// <summary>
        ///     Helper function to determine whether all GameTypes (Slot, Keno, Poker, etc) should be displayed
        /// </summary>
        private bool IsShowAllGameTypes()
        {
            return _selectedGameType == ResourceKeys.AllGameTypes;
        }

        /// <summary>
        ///     Notification when the operator changes the selection of a GameTheme (clicks the checkbox).  Filter the
        ///     GamePerformanceItems based on the new selection state.
        /// </summary>
        private void OnThemeSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is GamePerformanceGameTheme theme)
            {
                if (_deselectedGameThemes.ContainsKey(SelectedGameType))
                {
                    if (theme.Checked)
                    {
                        _deselectedGameThemes[SelectedGameType].Remove(theme.GameId);
                    }
                    else
                    {
                        _deselectedGameThemes[SelectedGameType].Add(theme.GameId);
                    }

                    var jsonText = JsonConvert.SerializeObject(_deselectedGameThemes);
                    PropertiesManager.SetProperty(
                        GamingConstants.OperatorMenuPerformancePageDeselectedGameThemes,
                        jsonText);
                }
            }

            UpdateGamePerformanceItems();
        }

        private void ShowMoreMetersPressed(object obj)
        {
            if (!(obj is GamePerformanceData gameData))
            {
                return;
            }

            var viewModel = new GamePerformanceMetersViewModel(
                gameData.GameId,
                gameData.GameNumber,
                gameData.Denomination);

            _dialogService.ShowInfoDialog<GamePerformanceMetersView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GamePerformanceMetersCaption));
        }
    }
}