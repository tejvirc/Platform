namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Input;
    using Application.Contracts;
    using Application.Contracts.Localization;
    using Application.Contracts.OperatorMenu;
    using Application.UI.OperatorMenu;
    using Contracts;
    using Contracts.Models;
    using Contracts.Tickets;
    using Hardware.Contracts.Ticket;
    using Kernel;
    using Localization.Properties;
    using Monaco.UI.Common.Extensions;
    using Views.OperatorMenu;

    /// <summary>
    ///     Defines the GameInfoViewModel class
    /// </summary>
    [CLSCompliant(false)]
    public class GameInfoViewModel : OperatorMenuPageViewModelBase
    {
        private const double NumberToPercentageDivisor = 10000D;

        private readonly IDialogService _dialogService;
        private readonly IGameOrderSettings _gameOrderSettings;

        private bool _allowPrintGamingMachineInfo;
        private bool _downButtonEnabled;
        private ObservableCollection<GameOrderData> _gameList = new ObservableCollection<GameOrderData>();
        private int _selectedIndex = -1;
        private GameOrderData _selectedItem;
        private string _selectedTag;
        private bool _upButtonEnabled;
        private readonly bool _showMode;
        private readonly bool _setGameOrderOnlyInShowMode;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameInfoViewModel" /> class.
        /// </summary>
        public GameInfoViewModel()
            : base(true)
        {
            var container = ServiceManager.GetInstance().GetService<IContainerService>();
            if (container != null)
            {
                _dialogService = ServiceManager.GetInstance().GetService<IDialogService>();
                _gameOrderSettings = container.Container.GetInstance<IGameOrderSettings>();
                _showMode = PropertiesManager.GetValue(ApplicationConstants.ShowMode, false);
            }

            _allowPrintGamingMachineInfo = true;

            SetGameOrderCommand = new RelayCommand<object>(SetGameOrder);
            _setGameOrderOnlyInShowMode = GetConfigSetting(OperatorMenuSetting.SetGameOrderOnlyInShowMode, false);
        }

        public string GameInfoTitle => Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameOrderTestMessage);

        public ICommand SetGameOrderCommand { get; }

        public bool SetGameOrderVisible => !_setGameOrderOnlyInShowMode || _showMode;

        /// <summary>
        ///     Gets or sets the game list
        /// </summary>
        public ObservableCollection<GameOrderData> GameList
        {
            get => _gameList;

            set
            {
                if (_gameList != value)
                {
                    _gameList = value;
                    OnPropertyChanged(nameof(GameList));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the selected item.
        /// </summary>
        public GameOrderData SelectedItem
        {
            get => _selectedItem;

            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        /// <summary>
        ///     Gets or sets the selected index.
        /// </summary>
        public int SelectedIndex
        {
            get => _selectedIndex;

            set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = value;
                    OnPropertyChanged(nameof(SelectedIndex));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the up button is enabled.
        /// </summary>
        public bool UpButtonEnabled
        {
            get => _upButtonEnabled;

            set
            {
                if (_upButtonEnabled != value)
                {
                    _upButtonEnabled = value;
                    OnPropertyChanged(nameof(UpButtonEnabled));
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the down button is enabled.
        /// </summary>
        public bool DownButtonEnabled
        {
            get => _downButtonEnabled;

            set
            {
                if (_downButtonEnabled != value)
                {
                    _downButtonEnabled = value;
                    OnPropertyChanged(nameof(DownButtonEnabled));
                }
            }
        }

        public bool SetTagsButtonVisible => Debugger.IsAttached;

        public string SelectedTag
        {
            get => _selectedTag;
            set
            {
                if (_selectedTag != value)
                {
                    _selectedTag = value;
                    OnPropertyChanged(nameof(SelectedTag));
                }
            }
        }

        public bool AllowPrintGamingMachineInfo
        {
            get => _allowPrintGamingMachineInfo;

            set
            {
                if (_allowPrintGamingMachineInfo != value)
                {
                    _allowPrintGamingMachineInfo = value;
                    OnPropertyChanged(nameof(AllowPrintGamingMachineInfo));
                }
            }
        }

        protected override void OnLoaded()
        {
            SelectedItem = null;

            EventBus.Subscribe<GameIconOrderChangedEvent>(this, HandleOrderChangedEvent);

            GameList = new ObservableCollection<GameOrderData>(LoadGames().OrderBy(GameOrder)); // made for VLT-6867
        }

        private IEnumerable<GameOrderData> LoadGames()
        {
            var games = PropertiesManager.GetValues<IGameDetail>(GamingConstants.Games);

            if (games != null)
            {
                return new List<GameOrderData>(
                    from game in games
                    from denom in game.SupportedDenominations
                    where game.Active
                    select new GameOrderData
                    {
                        GameId = game.Id,
                        Name = game.ThemeName,
                        Paytable = game.PaytableId,
                        Version = game.Version,
                        Installed = game.InstallDate,
                        ThemeId = game.ThemeId,
                        ThemeName = game.ThemeName,
                        GameTags = new ObservableCollection<string>(game.GameTags ?? new List<string>()),
                        TheoPaybackPct = game.MaximumPaybackPercent.ToDecimal(),
                        TheoPaybackPctDisplay =
                            $"{Localizer.For(CultureFor.Operator).GetString(ResourceKeys.TheoPaybackPctLabelText)}: {game.MaximumPaybackPercent.ToDecimal():p3}"
                    });
            }

            return new List<GameOrderData>();
        }

        protected override void OnUnloaded()
        {
            SelectedItem = null;
            GameList = null;
            DownButtonEnabled = false;
            UpButtonEnabled = false;
        }

        protected override IEnumerable<Ticket> GenerateTicketsForPrint(OperatorMenuPrintData dataType)
        {
            if (dataType != OperatorMenuPrintData.Main)
            {
                return null;
            }

            var ticketCreator = ServiceManager.GetInstance().TryGetService<IGameInfoTicketCreator>();

            return ticketCreator?.Create(GameList.ToList());
        }

        private void HandleOrderChangedEvent(GameIconOrderChangedEvent @event)
        {
            GameList = new ObservableCollection<GameOrderData>(GameList.OrderBy(GameOrder));
        }

        protected override void OnOperatorCultureChanged(OperatorCultureChangedEvent evt)
        {
            Execute.OnUIThread(
                () =>
                {
                    GameList.Clear();
                    GameList.AddRange(LoadGames().OrderBy(GameOrder));
                });
            base.OnOperatorCultureChanged(evt);
        }

        private int GameOrder(GameOrderData game)
        {
            return _gameOrderSettings.GetIconPositionPriority(game.ThemeId);
        }

        private void SetGameOrder(object obj)
        {
            var viewModel = new SetGameOrderViewModel();

            _dialogService.ShowDialog<SetGameOrderView>(this, viewModel, Localizer.For(CultureFor.Operator).GetString(ResourceKeys.GameOrderMessage));
        }
    }
}
