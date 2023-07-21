namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.MeterPage;
    using Aristocrat.Toolkit.Mvvm.Extensions;
    using CommunityToolkit.Mvvm.Input;
    using Contracts;
    using Contracts.Meters;
    using Kernel;
    using Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using Vgt.Client12.Application.OperatorMenu;

    public class GamePerformanceMetersViewModel : GameMetersViewModel, IModalDialogSaveViewModel
    {
        private readonly IGameDetail _game;

        private bool? _dialogResult;
        private IOperatorMenuLauncher _operatorMenuLauncher;

        public GamePerformanceMetersViewModel(int gameId, long gameNumber, decimal gameDenom)
        {
            var gameProvider = ServiceManager.GetInstance().GetService<IGameProvider>();
            _game = gameProvider.GetGame(gameId);
            GameDenom = gameDenom;
            GameNumber = gameNumber;

            EventBus.Subscribe<OperatorMenuExitingEvent>(this, HandleEvent);
            CancelCommand = new RelayCommand<object>(_ => Cancel());
            CancelButtonText = Localizer.For(CultureFor.Operator).GetString(ResourceKeys.Close);
        }

        public int GameId => _game.Id;

        public long GameNumber { get; set; }

        public string GameTheme => _game.ThemeName;

        public decimal GameDenom { get; set; }

        public string GamePaytable => _game.PaytableName;

        public decimal GameTheoreticalWeightedRTP { get; set; }

        public ObservableCollection<PerformanceWagerCategory> WagerCategoryMeters { get; set; }

        public bool ShowSaveButton => false;

        public bool ShowCancelButton => true;

        public string CancelButtonText { get; set; }

        public ICommand CancelCommand { get; }

        public bool? DialogResult
        {
            get => _dialogResult;

            protected set
            {
                if (_dialogResult != value)
                {
                    _dialogResult = value;
                    OnPropertyChanged(nameof(DialogResult));
                }
            }
        }

        public bool CanSave => false;

        public bool HasChanges()
        {
            return false;
        }

        public void Save()
        {
        }

        protected virtual void Cancel()
        {
            DialogResult = false;
        }

        protected override void OnLoaded()
        {
            _operatorMenuLauncher = ServiceManager.GetInstance().TryGetService<IOperatorMenuLauncher>();
            _operatorMenuLauncher?.PreventExit();
            base.OnLoaded();
        }

        protected override void LoadGameList()
        {
            // For this popup dialog, we only want to show meters for the game selected on the Performance page  
            Games = new ObservableCollection<IGameDetail> { _game };
            OnPropertyChanged(nameof(Games));
            SelectedGameIndex = 0;
        }

        protected override void OnUnloaded()
        {
            _operatorMenuLauncher?.AllowExit();
        }

        protected override void InitializeMeters()
        {
            if (SelectedGame == null)
            {
                return;
            }

            // This will occur each time a different game is selected
            var meterManager = ServiceManager.GetInstance().GetService<IGameMeterManager>();

            Execute.OnUIThread(
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
                        var label = meterNode.Name + "Label";

                        string meterDisplayName;

                        meterDisplayName = localizer.GetString(label, _ => meterDisplayName = meterNode.DisplayName);
                        if (!meterNode.DisplayName.IsEmpty() || meterNode.Name != "blank line")
                        {
                            try
                            {
                                var meter = meterManager.GetMeter(
                                    SelectedGame.Id,
                                    GameDenom.DollarsToMillicents(),
                                    meterNode.Name);
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

                    foreach (var meter in Meters)
                    {
                        MetersLeftColumn.Add(meter);
                    }

                    WagerCategoryMeters = new ObservableCollection<PerformanceWagerCategory>(
                        _game.WagerCategories.Select(
                            w => new PerformanceWagerCategory
                            {
                                Id = w.Id,
                                RTP = w.TheoPaybackPercent,
                                WageredMillicents = meterManager.GetMeter(
                                    GameId,
                                    GameDenom.DollarsToMillicents(),
                                    w.Id,
                                    GamingMeters.WagerCategoryWageredAmount).Lifetime
                            }
                        ));
                    OnPropertyChanged(nameof(WagerCategoryMeters));

                    var totalAmountIn = WagerCategoryMeters.Sum(d => d.WageredMillicents);

                    if (totalAmountIn > 0)
                    {
                        GameTheoreticalWeightedRTP =
                            WagerCategoryMeters.Sum(d => d.RTP * d.WageredMillicents) / totalAmountIn;
                    }
                    else
                    {
                        GameTheoreticalWeightedRTP = 0;
                    }

                    OnPropertyChanged(nameof(GameTheoreticalWeightedRTP));
                });
        }

        private void HandleEvent(OperatorMenuExitingEvent theEvent)
        {
            Execute.OnUIThread(Cancel);
        }
    }

    public class PerformanceWagerCategory
    {
        public string Id { get; set; }

        public decimal RTP { get; set; }

        public long WageredMillicents { get; set; }

        /// <summary> Gets or sets the credits in.</summary>
        public decimal Wagered => WageredMillicents.MillicentsToDollars();
    }
}
