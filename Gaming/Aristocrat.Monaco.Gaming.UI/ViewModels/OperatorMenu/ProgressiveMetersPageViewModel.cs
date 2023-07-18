namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.MeterPage;
    using Application.Contracts.OperatorMenu;
    using Application.UI.Events;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Common;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Progressives;
    using Contracts.Progressives.SharedSap;
    using Contracts.Tickets;
    using Factories;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using static DenomMetersPageViewModel;

    [CLSCompliant(false)]
    public class ProgressiveMetersPageViewModel : MetersPageViewModelBase
    {
        private readonly IProgressiveLevelProvider _progressiveProvider;
        private readonly IProgressiveMeterManager _progressiveMeterManager;
        private readonly ISharedSapProvider _sharedSap;
        private readonly bool _displayLpProgressives;
        private readonly bool _displaySapProgressives;
        private IGameDetail _selectedGame;
        private Denomination _selectedDenom;
        private string _selectedBetOption;
        private int _selectedGameIndex;
        private int _selectedDenomIndex;
        private int _selectedBetOptionIndex;
        private bool _hasEnabledProgressives;
        private bool _hasProgressives;
        private bool _viewBetOptionFilter;

        public ProgressiveMetersPageViewModel()
            : base(MeterNodePage.Progressives)
        {
            var serviceManager = ServiceManager.GetInstance();
            _progressiveProvider = serviceManager.GetService<IContainerService>()
                .Container.GetInstance<IProgressiveLevelProvider>();

            _sharedSap = serviceManager.GetService<IContainerService>()
                .Container.GetInstance<ISharedSapProvider>();

            _progressiveMeterManager = serviceManager.GetService<IProgressiveMeterManager>();

            PreviousGameCommand = new RelayCommand<object>(PreviousGame);
            NextGameCommand = new RelayCommand<object>(NextGame);

            PreviousDenomCommand = new RelayCommand<object>(PreviousDenom);
            NextDenomCommand = new RelayCommand<object>(NextDenom);

            PreviousBetOptionCommand = new RelayCommand<object>(PreviousBetOption);
            NextBetOptionCommand = new RelayCommand<object>(NextBetOption);

            BetOptions = new ObservableCollection<string>();
            Denoms = new ObservableCollection<Denomination>();
            SelectByGameNameAndDenomination = GetConfigSetting(OperatorMenuSetting.GameNameAndDenominationSelections, false);
            var allGames = new ObservableCollection<IGameDetail>(PropertiesManager.GetValues<IGameDetail>(GamingConstants.AllGames).OrderBy(game => game.Id));
            if (SelectByGameNameAndDenomination)
            {
                Games = new ObservableCollection<IGameDetail>(
                    PropertiesManager.GetValues<IGameDetail>(GamingConstants.AllGames)
                        .Where(game => game.Enabled)
                        .DistinctBy(game => game.ThemeName).OrderBy(game => game.Id));
            }
            else
            {
                Games = allGames;
            }

            _displayLpProgressives = GetConfigSetting(OperatorMenuSetting.ShowLinkedProgressiveMeters, true);
            _displaySapProgressives = GetConfigSetting(OperatorMenuSetting.ShowSAPMeters, true);
        }

        public bool HasProgressivesButNoneEnabled => !HasEnabledProgressives && HasProgressives;

        public bool HasEnabledProgressives
        {
            get => _hasEnabledProgressives;
            private set
            {
                SetProperty(ref _hasEnabledProgressives, value, nameof(HasEnabledProgressives));
                OnPropertyChanged(nameof(HasProgressivesButNoneEnabled));
            }
        }

        public bool HasProgressives
        {
            get => _hasProgressives;
            private set
            {
                SetProperty(ref _hasProgressives, value, nameof(HasProgressives));
                OnPropertyChanged(nameof(HasProgressivesButNoneEnabled));
            }
        }

        public ObservableCollection<ProgressiveDisplayMeter> ProgressiveDetailMeters { get; } =
            new ObservableCollection<ProgressiveDisplayMeter>();

        public ICommand PreviousGameCommand { get; }

        public ICommand NextGameCommand { get; }

        public bool PreviousGameIsEnabled => SelectedGameIndex > 0 && !DataEmpty;

        public bool NextGameIsEnabled => SelectedGameIndex < Games.Count - 1 && !DataEmpty;

        public ObservableCollection<IGameDetail> Games { get; set; }

        public override bool DataEmpty => (Games?.Count ?? 0) == 0;

        public bool SelectByGameNameAndDenomination { get; }

        public IGameDetail SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;

                Denoms.Clear();
                foreach (var d in _selectedGame.Denominations)
                {
                    Denoms.Add(new Denomination(d.Value, d.Value.MillicentsToDollars().FormattedCurrencyString(false, GetCurrencyDisplayCulture())));
                }

                UpdateBetOptions();

                OnPropertyChanged(nameof(Denoms));
                SelectedDenomIndex = 0;

                OnPropertyChanged(nameof(SelectedGame));
                InitializeMeters();
            }
        }

        protected override void UpdateStatusText()
        {
            if (!HasEnabledProgressives)
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoProgressiveLevelsAdded)));
            }
            else
            {
                base.UpdateStatusText();
            }
        }

        private void UpdateBetOptions()
        {
            BetOptions.Clear();

            if (SelectByGameNameAndDenomination)
            {
                return;
            }

            // check if we have different level pool for betOptions for the levels to be displayed
            var betOptionList = GetProgressiveLevels()
            .Where(level => level.GameId == _selectedGame?.Id
                            && level.Denomination.Contains(SelectedDenom.Millicents) && level.BetOption != null)
            .ToArray().Select(x => x.BetOption).Distinct().ToList();

            if (betOptionList.Count != 0)
            {
                ViewBetOptionFilter = true;
                foreach (var betOption in betOptionList)
                {
                    BetOptions.Add(betOption);
                }
            }
            else
            {
                ViewBetOptionFilter = false;
            }

            SelectedBetOptionIndex = 0;
        }

        public int SelectedGameIndex
        {
            get => _selectedGameIndex;
            set
            {
                if (value < 0 || value >= Games.Count)
                {
                    return;
                }

                SelectedGame = Games[value];
                _selectedGameIndex = value;

                OnPropertyChanged(nameof(SelectedGameIndex));
                OnPropertyChanged(nameof(PreviousGameIsEnabled));
                OnPropertyChanged(nameof(NextGameIsEnabled));
            }
        }

        public ICommand PreviousDenomCommand { get; }

        public ICommand NextDenomCommand { get; }

        public bool PreviousDenomIsEnabled => SelectedDenomIndex > 0 && !DataEmpty;

        public bool NextDenomIsEnabled => SelectedDenomIndex < Denoms.Count - 1 && !DataEmpty;

        public ObservableCollection<Denomination> Denoms { get; set; }

        public Denomination SelectedDenom
        {
            get => _selectedDenom;
            set
            {
                _selectedDenom = value;
                OnPropertyChanged(nameof(SelectedDenom));
                InitializeMeters();
            }
        }

        public int SelectedDenomIndex
        {
            get => _selectedDenomIndex;
            set
            {
                if (value < 0 || value >= Denoms.Count)
                {
                    return;
                }

                _selectedDenomIndex = value;
                SelectedDenom = Denoms[value];
                OnPropertyChanged(nameof(SelectedDenomIndex));
                OnPropertyChanged(nameof(PreviousDenomIsEnabled));
                OnPropertyChanged(nameof(NextDenomIsEnabled));
            }
        }

        public ICommand PreviousBetOptionCommand { get; }

        public ICommand NextBetOptionCommand { get; }

        public bool PreviousBetOptionIsEnabled => SelectedBetOptionIndex > 0 && !DataEmpty;

        public bool NextBetOptionIsEnabled => SelectedBetOptionIndex < BetOptions.Count - 1 && !DataEmpty;

        public ObservableCollection<string> BetOptions { get; set; }

        public int SelectedBetOptionIndex
        {
            get => _selectedBetOptionIndex;
            set
            {
                if (value < 0 || value >= BetOptions.Count)
                {
                    return;
                }

                _selectedBetOptionIndex = value;
                SelectedBetOption = BetOptions[value];
                OnPropertyChanged(nameof(SelectedBetOptionIndex));
                OnPropertyChanged(nameof(PreviousBetOptionIsEnabled));
                OnPropertyChanged(nameof(NextBetOptionIsEnabled));
            }
        }

        public string SelectedBetOption
        {
            get => _selectedBetOption;
            set
            {
                _selectedBetOption = value;
                OnPropertyChanged(nameof(SelectedBetOption));
                InitializeMeters();
            }
        }

        public bool ViewBetOptionFilter
        {
            get => _viewBetOptionFilter;
            set
            {
                if (value != _viewBetOptionFilter)
                {
                    _viewBetOptionFilter = value;
                    OnPropertyChanged(nameof(ViewBetOptionFilter));
                }
            }
        }

        public event EventHandler<EventArgs> OnShouldRegenerateColumns;

        protected void LoadProgressiveMeters(ProgressiveLevel[] progressiveLevels)
        {
            HasProgressives = GetHasProgressives(progressiveLevels);

            if (!HasProgressives)
            {
                return;
            }
            Execute.OnUIThread(
                () =>
                {
                    // Per each progressive level which:
                    // 1. Initializes a list that represents a row of data that we wish to show in the GUI.
                    // 2. Gets all of the required meters from DisplayMeters.config.xml that need to be loaded as part of this row
                    // 3. Store the meters in the list from 1. and expose it to be consumed by the GUI.
                    foreach (var progressiveLevel in progressiveLevels)
                    {
                        var sharedHiddenTotal = 0L;

                        if (_sharedSap.ViewSharedSapLevel(
                            progressiveLevel.AssignedProgressiveId.AssignedProgressiveKey,
                            out var sharedLevel))
                        {
                            progressiveLevel.CurrentValue = sharedLevel.CurrentValue;
                            progressiveLevel.Residual = sharedLevel.Residual;
                            progressiveLevel.Overflow = sharedLevel.Overflow;
                            progressiveLevel.OverflowTotal = sharedLevel.OverflowTotal;
                            progressiveLevel.HiddenIncrementRate = sharedLevel.HiddenIncrementRate;
                            progressiveLevel.HiddenValue = sharedLevel.HiddenValue;
                            sharedHiddenTotal = sharedLevel.HiddenTotal;
                        }

                        var collectionOfMeters = new ObservableCollection<DisplayMeter>();
                        foreach (var meterNode in MeterNodes)
                        {
                            collectionOfMeters.Add(_progressiveMeterManager.Build(progressiveLevel, meterNode, ShowLifetime, SelectedDenom.Millicents, sharedHiddenTotal));
                        }
                        ProgressiveDetailMeters.Add(new ProgressiveDisplayMeter(progressiveLevel.ProgressivePackName, ShowLifetime, collectionOfMeters));
                    }
                });
        }

        protected ProgressiveLevel[] LoadProgressivesByGameNameAndDenomination()
        {
            return GetProgressiveLevels()
                .Where(level => level.GameId == SelectedGame?.Id && level.Denomination.Contains(SelectedDenom.Millicents))
                .ToArray();
        }

        protected ProgressiveLevel[] LoadProgressivesByGameNameDenominationAndBetOption()
        {
            return GetProgressiveLevels()
                .Where(level => level.GameId == SelectedGame?.Id
                                && (level.BetOption is null || !level.BetOption.Any() || (ViewBetOptionFilter && level.BetOption == SelectedBetOption))
                                && level.Denomination.Contains(SelectedDenom.Millicents))
                .ToArray();
        }

        protected override void InitializeMeters()
        {
            ClearMeters();

            if (!HasEnabledProgressives)
            {
                return;
            }

            LoadProgressiveMeters(
                SelectByGameNameAndDenomination
                    ? LoadProgressivesByGameNameAndDenomination()
                    : LoadProgressivesByGameNameDenominationAndBetOption());
        }

        protected override void UpdateMeters()
        {
            foreach (var meter in ProgressiveDetailMeters)
            {
                meter.ShowLifetime = ShowLifetime;

                foreach (var detail in meter.Details)
                {
                    detail.ShowLifetime = ShowLifetime;
                }
            }
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            switch (dataType)
            {
                case OperatorMenuPrintData.Main:
                    var ticketCreator = ServiceManager.GetInstance()
                        .TryGetService<IProgressiveSetupAndMetersTicketCreator>();
                    tickets = ticketCreator.CreateProgressiveSetupAndMetersTicket(
                        SelectedGame,
                        SelectedDenom.Millicents);
                    break;
            }

            return tickets;
        }

        protected override void OnLoaded()
        {
            SelectedGameIndex = 0;
            SelectedDenomIndex = 0;
            SelectedBetOptionIndex = 0;
            base.OnLoaded();

            // Refresh each load in case game configurations have changed
            HasEnabledProgressives = IsEnabledProgressives();

            // If we don't check on the initial load, then no meters will be shown
            // until the user changes pages.
            if (HasEnabledProgressives)
            {
                InitializeMeters();
            }
            else
            {
                EventBus.Publish(new OperatorMenuWarningMessageEvent(Localizer.For(CultureFor.Operator).GetString(ResourceKeys.NoProgressiveLevelsAdded)));
            }

            OnShouldRegenerateColumns?.Invoke(this, new EventArgs());
        }

        protected override void DisposeInternal()
        {
            ClearMeters();

            base.DisposeInternal();
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            Execute.OnUIThread(() =>
            {
                if (Games.Any())
                {
                    SelectedGame = Games.FirstOrDefault();
                }

                UpdateStatusText();
            });

            OnShouldRegenerateColumns?.Invoke(this, EventArgs.Empty); // Used by page code-behind

            base.OnOperatorCultureChanged(evt);
        }

        private void NextGame(object sender)
        {
            // cycle on last game
            SelectedGameIndex = SelectedGameIndex == Games.Count - 1 ? 0 : SelectedGameIndex + 1;
        }

        private void PreviousGame(object sender)
        {
            // cycle on first game
            SelectedGameIndex = SelectedGameIndex == 0 ? Games.Count - 1 : SelectedGameIndex - 1;
        }

        private void NextDenom(object sender)
        {
            // cycle on last game
            SelectedDenomIndex = SelectedDenomIndex == Denoms.Count - 1 ? 0 : SelectedDenomIndex + 1;
        }

        private void PreviousBetOption(object sender)
        {
            // cycle on first game
            SelectedBetOptionIndex = SelectedBetOptionIndex == 0 ? BetOptions.Count - 1 : SelectedBetOptionIndex - 1;
        }

        private void NextBetOption(object sender)
        {
            // cycle on last game
            SelectedBetOptionIndex = SelectedBetOptionIndex == BetOptions.Count - 1 ? 0 : SelectedBetOptionIndex + 1;
        }

        private void PreviousDenom(object sender)
        {
            // cycle on first game
            SelectedDenomIndex = SelectedDenomIndex == 0 ? Denoms.Count - 1 : SelectedDenomIndex - 1;
        }

        private void ClearMeters()
        {
            foreach (var meter in ProgressiveDetailMeters)
            {
                meter.Dispose();
            }

            ProgressiveDetailMeters.Clear();
        }

        private bool IsEnabledProgressives()
        {
            return GetProgressiveLevels().Any();
        }

        private bool GetHasProgressives(ProgressiveLevel[] progressiveLevels)
        {
            if (_displayLpProgressives && _displaySapProgressives)
            {
                return progressiveLevels.Any();
            }
            else if (_displayLpProgressives)
            {
                return progressiveLevels.Any(p => p.LevelType == ProgressiveLevelType.LP);
            }
            else if (_displaySapProgressives)
            {
                return progressiveLevels.Any(p => p.LevelType == ProgressiveLevelType.Sap);
            }

            return false;
        }

        private IEnumerable<ProgressiveLevel> GetProgressiveLevels()
        {
            if (_displayLpProgressives && _displaySapProgressives)
            {
                return _progressiveProvider.GetProgressiveLevels();
            }
            else if (_displayLpProgressives)
            {
                return _progressiveProvider.GetProgressiveLevels().Where(p => p.LevelType == ProgressiveLevelType.LP);
            }
            else if (_displaySapProgressives)
            {
                return _progressiveProvider.GetProgressiveLevels().Where(p => p.LevelType == ProgressiveLevelType.Sap);
            }

            return new List<ProgressiveLevel>();
        }
    }
}
