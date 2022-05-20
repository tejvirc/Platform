namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Contracts;
    using Contracts.Models;
    using Diagnostics;
    using Kernel;
    using Localization.Properties;
    using Models;
    using MVVM.Command;

    public class CombinationTestViewModel : OperatorMenuDiagnosticPageViewModelBase
    {
        private readonly IGameDiagnostics _diagnostics;
        private readonly IGameProvider _gameProvider;

        private ObservableCollection<GameComboInfo> _games;
        private GameComboInfo _selectedGame;

        public CombinationTestViewModel()
        {
            var container = ServiceManager.GetInstance().GetService<IContainerService>();
            if (container != null)
            {
                _gameProvider = container.Container.GetInstance<IGameProvider>();
                _diagnostics = container.Container.GetInstance<IGameDiagnostics>();
            }

            ComboTestCommand = new ActionCommand<object>(_ => LaunchCombinationTest(), _ => SelectedGame != null && InputEnabled);

            Games = new ObservableCollection<GameComboInfo>();
        }

        public IActionCommand ComboTestCommand { get; }

        public GameComboInfo SelectedGame
        {
            get => _selectedGame;
            set
            {
                if (_selectedGame != value)
                {
                    _selectedGame = value;
                    RaisePropertyChanged(nameof(SelectedGame));
                    ComboTestCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<GameComboInfo> Games
        {
            get => _games;
            set
            {
                _games = value;
                RaisePropertyChanged(nameof(Games));
            }
        }

        protected override void OnInputStatusChanged()
        {
            ComboTestCommand.RaiseCanExecuteChanged();
        }

        protected override void OnInputEnabledChanged()
        {
            ComboTestCommand.RaiseCanExecuteChanged();
        }

        protected override void InitializeData()
        {
            Games = new ObservableCollection<GameComboInfo>(
                _gameProvider.GetGames().Where(g => g.GameType == GameType.Slot)
                    .SelectMany(
                        game => game.Denominations,
                        (g, d) => new GameComboInfo
                        {
                            Id = g.Id,
                            UniqueId = d.Id,
                            ThemeId = g.ThemeId,
                            PaytableId = g.PaytableId,
                            Denomination = d.Value.MillicentsToDollars(),
                            Name = g.ThemeName
                        }).OrderBy(x => x.UniqueId));

            SelectedGame = null;
        }

        private void LaunchCombinationTest()
        {
            if (SelectedGame == null)
            {
                return;
            }

            PreventOperatorMenuExit();

            _diagnostics.Start(
                SelectedGame.Id,
                SelectedGame.Denomination.DollarsToMillicents(),
                Localizer.For(CultureFor.Operator).GetString(ResourceKeys.CombinationTestTitle),
                new CombinationTestContext(),
                true);
        }
    }
}