namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.MeterPage;
    using Application.Contracts.OperatorMenu;
    using Application.UI.MeterPage;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Meters;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using MVVM;
    using MVVM.Command;
    using Common;
    using Views.OperatorMenu;
    using static DenomMetersPageViewModel;

    /// <summary>
    ///     Interaction logic for GamesMetersPage.xaml
    /// </summary>
    /// <remarks>
    ///     The meters that are displayed in the Audit Menu for each game are defined the Jurisdiction.addin.xml file
    ///     within the Extension path = "/Application/OperatorMenu/DisplayMeters/MeterNode[@Page="Denom"]" block
    /// </remarks>
    public class GameMetersViewModel : MetersPageViewModelBase
    {
        private readonly IDialogService _dialogService;

        private bool _gameButtonsEnabled = true;
        private IGameDetail _selectedGame;
        private Denomination _selectedDenom;
        private ObservableCollection<IGameDetail> _allGames;
        private int _selectedGameIndex;
        private int _selectedDenomIndex;

        public GameMetersViewModel()
            : base(MeterNodePage.Game, true)
        {
            if (!InDesigner)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
            }

            GameButtonsEnabled = true;
            WagerCategoryMetersVisible = GetConfigSetting(OperatorMenuSetting.ShowWagerCategoryMeters, true);
            SelectByGameNameAndDenomination = GetConfigSetting(
                OperatorMenuSetting.GameNameAndDenominationSelections,
                false);
            PreviousGameCommand = new ActionCommand<object>(PreviousGame);
            NextGameCommand = new ActionCommand<object>(NextGame);
            PreviousDenomCommand = new ActionCommand<object>(PreviousDenom);
            NextDenomCommand = new ActionCommand<object>(NextDenom);
            DisplayCategoriesCommand = new ActionCommand<object>(ShowWagerCategoryMeters);
        }

        public ICommand PreviousGameCommand { get; }

        public ICommand NextGameCommand { get; }

        public ICommand PreviousDenomCommand { get; }

        public ICommand NextDenomCommand { get; }

        public ICommand DisplayCategoriesCommand { get; }

        public bool PreviousGameIsEnabled => SelectedGameIndex > 0 && !DataEmpty;

        public bool NextGameIsEnabled => SelectedGameIndex < Games.Count - 1 && !DataEmpty;

        public bool PreviousDenomIsEnabled => SelectedDenomIndex > 0 && !DataEmpty;

        public bool NextDenomIsEnabled => SelectedDenomIndex < Denoms.Count - 1 && !DataEmpty;

        public ObservableCollection<IGameDetail> Games { get; set; } = new ObservableCollection<IGameDetail>();

        public ObservableCollection<Denomination> Denoms { get; set; } = new ObservableCollection<Denomination>();

        public bool GameButtonsEnabled
        {
            get => _gameButtonsEnabled;
            set
            {
                _gameButtonsEnabled = value;
                RaisePropertyChanged(nameof(GameButtonsEnabled));
                RaisePropertyChanged(nameof(PreviousGameIsEnabled));
                RaisePropertyChanged(nameof(NextGameIsEnabled));
            }
        }

        public bool DisplayCategoriesButtonEnabled => _selectedGame != null;

        public override bool DataEmpty => (Games?.Count ?? 0) == 0;

        public IGameDetail SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                RaisePropertyChanged(nameof(SelectedGame));
                RaisePropertyChanged(nameof(DisplayCategoriesButtonEnabled));
                InitializeMeters();
                LoadDenoms();
            }
        }

        public Denomination SelectedDenom
        {
            get => _selectedDenom;
            set
            {
                _selectedDenom = value;
                RaisePropertyChanged(nameof(SelectedDenom));
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
                RaisePropertyChanged(nameof(SelectedDenomIndex));
                RaisePropertyChanged(nameof(PreviousDenomIsEnabled));
                RaisePropertyChanged(nameof(NextDenomIsEnabled));
            }
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

                _selectedGameIndex = value;
                SelectedGame = Games[value];
                RaisePropertyChanged(nameof(PreviousGameIsEnabled));
                RaisePropertyChanged(nameof(NextGameIsEnabled));
            }
        }

        public bool WagerCategoryMetersVisible { get; set; }

        public bool SelectByGameNameAndDenomination { get; set; }

        protected override void InitializeMeters()
        {
            if (SelectedGame == null)
            {
                return;
            }

            // This will occur each time a different game is selected
            var meterManager = ServiceManager.GetInstance().GetService<IGameMeterManager>();

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    foreach (var meter in Meters)
                    {
                        meter.Dispose();
                    }

                    Meters.Clear();
                    MetersLeftColumn.Clear();
                    MetersRightColumn.Clear();

                    var localizer = Localizer.For(CultureFor.Operator);

                    foreach (var meterNode in MeterNodes)
                    {
                        string meterDisplayName = localizer.GetString(
                            meterNode.DisplayNameKey,
                            _ => meterDisplayName = meterNode.DisplayName);

                        if (meterNode.DisplayName.IsEmpty() && meterNode.Name == "blank line")
                        {
                            var lifetime = new BlankDisplayMeter(ShowLifetime, meterNode.Order);
                            Meters.Add(lifetime);
                        }
                        else
                        {
                            try
                            {
                                if (SelectByGameNameAndDenomination)
                                {
                                    IList<IMeter> metersToBeAggregated = _allGames
                                        .Where(game => game.ThemeId == SelectedGame.ThemeId).Select(
                                            gameVariation => meterManager.GetMeter(
                                                gameVariation.Id,
                                                SelectedDenom.Millicents,
                                                meterNode.Name)).ToList();
                                    Meters.Add(
                                        new AggregateDisplayMeter(
                                            meterDisplayName ?? meterNode.DisplayName,
                                            metersToBeAggregated,
                                            ShowLifetime,
                                            metersToBeAggregated.First().Classification,
                                            meterNode.Order,
                                            UseOperatorCultureForCurrencyFormatting));
                                }
                                else
                                {
                                    var meter = meterManager.GetMeter(SelectedGame.Id, meterNode.Name);
                                    Meters.Add(
                                        new DisplayMeter(
                                            meterDisplayName ?? meterNode.DisplayName,
                                            meter,
                                            ShowLifetime,
                                            meterNode.Order,
                                            true,
                                            false,
                                            UseOperatorCultureForCurrencyFormatting));
                                }
                            }
                            catch (MeterNotFoundException)
                            {
                                Meters.Add(
                                    new DisplayMeter(
                                        meterDisplayName ?? meterNode.DisplayName,
                                        null,
                                        ShowLifetime,
                                        meterNode.Order,
                                        true,
                                        false,
                                        UseOperatorCultureForCurrencyFormatting));

                                Logger.ErrorFormat("Meter not found: {0}", meterNode.Name);
                            }
                        }
                    }

                    SplitMeters();
                });
        }

        protected override void UpdateMeters()
        {
            InitializeMeters();
        }

        protected override void UpdatePrinterButtons()
        {
            RaisePropertyChanged(nameof(GameButtonsEnabled));
            RaisePropertyChanged(nameof(PreviousGameIsEnabled));
            RaisePropertyChanged(nameof(NextGameIsEnabled));
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            IEnumerable<Ticket> tickets = null;

            if (dataType == OperatorMenuPrintData.Main)
            {
                tickets = GetGameMeterTicket();
            }

            return tickets;
        }

        protected override void OnLoaded()
        {
            LoadGameList();
            GameButtonsEnabled = true;
            base.OnLoaded();
        }

        private List<Ticket> GetGameMeterTicket()
        {
            var ticketCreator = ServiceManager.GetInstance().TryGetService<IGameMetersTicketCreator>();
            List<Ticket> tickets = null;
            if (ticketCreator is null || SelectedGame is null)
            {
                return tickets;
            }

            var printMeters = Meters.Where(m => m.Meter != null && (ShowLifetime || m.DisplayPeriod))
                .Select(m => new Tuple<IMeter, string>(m.Meter, m.Name)).ToList();

            tickets = ticketCreator.Create(SelectedGame, printMeters, ShowLifetime);
            return tickets;
        }

        private void NextGame(object sender)
        {
            // cycle on last game
            SelectedGameIndex = SelectedGameIndex == Games.Count - 1
                ? 0
                : SelectedGameIndex + 1;
        }

        private void PreviousGame(object sender)
        {
            // cycle on first game
            SelectedGameIndex = SelectedGameIndex == 0
                ? Games.Count - 1
                : SelectedGameIndex - 1;
        }

        private void PreviousDenom(object sender)
        {
            // cycle on first game
            SelectedDenomIndex = SelectedDenomIndex == 0 ? Denoms.Count - 1 : SelectedDenomIndex - 1;
        }

        private void NextDenom(object sender)
        {
            // cycle on last game
            SelectedDenomIndex = SelectedDenomIndex == Denoms.Count - 1 ? 0 : SelectedDenomIndex + 1;
        }

        private void LoadDenoms()
        {
            if (!SelectByGameNameAndDenomination)
            {
                return;
            }

            Denoms.Clear();
            foreach (var d in _selectedGame.Denominations)
            {
                Denoms.Add(new Denomination(d.Value, d.Value.MillicentsToDollars().FormattedCurrencyString(false, GetCurrencyDisplayCulture())));
            }

            SelectedDenomIndex = 0;
        }

        private void ShowWagerCategoryMeters(object sender)
        {
            var viewModel = new WagerCategoryMetersViewModel(SelectedGame, ShowLifetime);

            _dialogService.ShowInfoDialog<WagerCategoryMetersView>(
                this,
                viewModel,
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.WagerCategoryTitle));
        }

        protected virtual void LoadGameList()
        {
            // This need to include all games that have ever been active on the cabinet.
            _allGames = new ObservableCollection<IGameDetail>(
                PropertiesManager.GetValues<IGameDetail>(GamingConstants.AllGames).OrderBy(game => game.Id));
            Games = SelectByGameNameAndDenomination
                ? new ObservableCollection<IGameDetail>(
                    PropertiesManager.GetValues<IGameDetail>(GamingConstants.AllGames)
                        .DistinctBy(game => game.ThemeName).OrderBy(game => game.Id))
                : _allGames;
            RaisePropertyChanged(nameof(Games));
            SelectedGameIndex = 0;
        }
    }
}