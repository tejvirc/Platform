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

    public class BingoInfoTestToolLoader : IBingoPresentationLoader
    {
        private readonly IEventBus _eventBus;
        private readonly IBingoDisplayConfigurationProvider _configurationProvider;
        private readonly IPropertiesManager _properties;
        private readonly IDispatcher _dispatcher;

        public BingoInfoTestToolLoader(
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider configurationProvider,
            IPropertiesManager properties,
            IDispatcher dispatcher)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
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
                    new BingoInfoTestToolTab
                    {
                        ViewModel = new BingoInfoTestToolViewModel(_eventBus, _configurationProvider)
                    }));
        }
    }
}
#endif