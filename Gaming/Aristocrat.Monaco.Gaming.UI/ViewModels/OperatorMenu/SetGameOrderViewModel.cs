using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aristocrat.Monaco.Application.UI.OperatorMenu;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Gaming.Contracts.Models;
using Aristocrat.Monaco.Kernel;
using Aristocrat.MVVM.Command;

namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    public class SetGameOrderViewModel : OperatorMenuSaveViewModelBase
    {
        private ObservableCollection<GameOrderData> _gameList = new ObservableCollection<GameOrderData>();
        private GameOrderData _selectedItem;
        private readonly IGameOrderSettings _gameOrderSettings;
        private bool _isDirty;

        public SetGameOrderViewModel()
        {
            var container = ServiceManager.GetInstance().GetService<IContainerService>();
            if (container != null)
            {
                _gameOrderSettings = container.Container.GetInstance<IGameOrderSettings>();
            }

            UpCommand = new RelayCommand<object>(UpPressed, o => SelectedItem != null && GameList.IndexOf(SelectedItem) != 0);
            DownCommand = new RelayCommand<object>(DownPressed, o => SelectedItem != null && GameList.IndexOf(SelectedItem) != GameList.Count - 1);
        }

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

        public GameOrderData SelectedItem
        {
            get => _selectedItem;

            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    UpCommand.NotifyCanExecuteChanged();
                    DownCommand.NotifyCanExecuteChanged();
                }
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            SelectedItem = null;

            GameList = new ObservableCollection<GameOrderData>(LoadGames().OrderBy(GameOrder));
        }

        private int GameOrder(GameOrderData game) => _gameOrderSettings.GetIconPositionPriority(game.ThemeId);

        public RelayCommand<object> UpCommand { get; }
        public RelayCommand<object> DownCommand { get; }

        private bool IsDirty
        {
            get => _isDirty;
            set
            {
                if (_isDirty != value)
                {
                    _isDirty = value;
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public override void Save()
        {
            _gameOrderSettings.SetIconOrder(GameList.Select(o => o.ThemeId), true);
        }

        private List<GameOrderData> LoadGames()
        {
            var games = PropertiesManager.GetValues<IGameDetail>(GamingConstants.Games);
            var uniqueGames = new List<GameOrderData>();
            if (games != null)
            {
                var themeIds = games.Select(o => o.ThemeId).Distinct();
                foreach (var themeId in themeIds)
                {
                    var game = games.First(o => o.ThemeId == themeId);
                    var gameOrderData = new GameOrderData()
                    {
                        Name = game.ThemeName,
                        ThemeId = game.ThemeId,
                    };
                    uniqueGames.Add(gameOrderData);
                }
            }

            return uniqueGames;
        }

        private void UpPressed(object obj)
        {
            var oldIndex = GameList.IndexOf(SelectedItem);
            var newIndex = oldIndex - 1;
            GameList.Move(oldIndex, newIndex);
            IsDirty = true;
            UpCommand.NotifyCanExecuteChanged();
        }

        private void DownPressed(object obj)
        {
            var oldIndex = GameList.IndexOf(SelectedItem);
            var newIndex = oldIndex + 1;
            GameList.Move(oldIndex, newIndex);
            IsDirty = true;
            DownCommand.NotifyCanExecuteChanged();
        }

        public override bool HasChanges()
        {
            return IsDirty;
        }
    }
}
