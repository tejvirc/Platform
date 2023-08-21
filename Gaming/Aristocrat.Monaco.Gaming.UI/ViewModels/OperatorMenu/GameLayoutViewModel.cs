namespace Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu
{
    using Application.UI.OperatorMenu;
    using Contracts;
    using Kernel;
    using MVVM.Command;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Input;

    public class GameLayoutViewModel : OperatorMenuSaveViewModelBase
    {
        private readonly IGameOrderSettings _gameOrderSettings;

        public GameLayoutViewModel()
        {
            MoveToFirstCommand = new ActionCommand<IGameDetail>(MoveToFirst);
            MoveLeftCommand = new ActionCommand<IGameDetail>(MoveLeft);
            MoveRightCommand = new ActionCommand<IGameDetail>(MoveRight);
            MoveToLastCommand = new ActionCommand<IGameDetail>(MoveToLast);

            _gameOrderSettings = ServiceManager.GetInstance().GetService<IGameOrderSettings>();

            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var games = properties.GetValues<IGameDetail>(GamingConstants.Games).Where(g => g.Enabled).ToList();
            
            var config = (LobbyConfiguration)properties.GetProperty(GamingConstants.LobbyConfig, null);
            var activeLocaleCode = config.LocaleCodes[0];

            Games = new ObservableCollection<GameLayoutItem>(
                games.OrderBy(GameOrder).Select(g => new GameLayoutItem(g, activeLocaleCode)));
        }

        public ICommand MoveToFirstCommand { get; }

        public ICommand MoveLeftCommand { get; }

        public ICommand MoveRightCommand { get; }

        public ICommand MoveToLastCommand { get; }

        public ObservableCollection<GameLayoutItem> Games { get; }

        public override void Save()
        {
            _gameOrderSettings.SetIconOrder(Games.Select(g => g.GameDetail.ThemeId), true);
        }

        private int GameOrder(IGameDetail game)
        {
            return _gameOrderSettings.GetIconPositionPriority(game.ThemeId);
        }

        private void MoveToFirst(IGameDetail game)
        {
            var layoutItem = Games.FirstOrDefault(g => g.GameDetail == game);
            if (Games.IndexOf(layoutItem) != 0)
            {
                Games.Remove(layoutItem);
                Games.Insert(0, layoutItem);
            }
            RaisePropertyChanged(nameof(Games));
        }

        private void MoveLeft(IGameDetail game)
        {
            var layoutItem = Games.FirstOrDefault(g => g.GameDetail == game);
            var index = Games.IndexOf(layoutItem);
            if (index > 0)
            {
                Games.Remove(layoutItem);
                Games.Insert(index - 1, layoutItem);
            }
            RaisePropertyChanged(nameof(Games));
        }

        private void MoveRight(IGameDetail game)
        {
            var layoutItem = Games.FirstOrDefault(g => g.GameDetail == game);
            var index = Games.IndexOf(layoutItem);
            if (index < Games.Count-1)
            {
                Games.Remove(layoutItem);
                Games.Insert(index + 1, layoutItem);
            }
            RaisePropertyChanged(nameof(Games));
        }

        private void MoveToLast(IGameDetail game)
        {
            var layoutItem = Games.FirstOrDefault(g => g.GameDetail == game);
            var index = Games.IndexOf(layoutItem);
            if (index < Games.Count - 1)
            {
                Games.Remove(layoutItem);
                Games.Add(layoutItem);
            }
            RaisePropertyChanged(nameof(Games));
        }

        public class GameLayoutItem
        {
            private readonly string _activeLocaleCode;

            public GameLayoutItem(IGameDetail detail, string activeLocaleCode)
            {
                GameDetail = detail;
                _activeLocaleCode = activeLocaleCode;
            }

            public IGameDetail GameDetail { get; }

            public string IconPath =>
                GameDetail.LocaleGraphics.ContainsKey(_activeLocaleCode)
                    ? GameDetail.LocaleGraphics[_activeLocaleCode].SmallIcon
                    : (GameDetail.LocaleGraphics.Any()
                        ? GameDetail.LocaleGraphics.First().Value.SmallIcon
                        : string.Empty);
        }
    }
}
