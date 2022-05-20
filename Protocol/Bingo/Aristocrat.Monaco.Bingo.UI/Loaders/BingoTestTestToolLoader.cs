#if !RETAIL
namespace Aristocrat.Monaco.Bingo.UI.Loaders
{
    using System;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Monaco.Common;
    using ViewModels.TestTool;
    using Views.TestTool;

    public class BingoTestTestToolLoader : IBingoPresentationLoader
    {
        private readonly IEventBus _eventBus;
        private readonly IGamePlayState _gamePlay;
        private readonly IPropertiesManager _properties;
        private readonly IDispatcher _dispatcher;

        public BingoTestTestToolLoader(
            IEventBus eventBus,
            IGamePlayState gamePlay,
            IPropertiesManager properties,
            IDispatcher dispatcher)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gamePlay = gamePlay ?? throw new ArgumentNullException(nameof(gamePlay));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        }

        public void LoadPresentation()
        {
            var showTestTool = _properties.GetValue(Constants.ShowTestTool, Constants.False);
            if (!string.Equals(showTestTool, Constants.True, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            _dispatcher.ExecuteOnUIThread(LoadUI);
        }

        private void LoadUI()
        {
            _eventBus.Publish(
                new TestToolPluginEvent(
                    new BingoTestTestToolTab
                    {
                        ViewModel = new BingoTestTestToolViewModel(_eventBus, _gamePlay, _properties)
                    }));
        }
    }
}
#endif