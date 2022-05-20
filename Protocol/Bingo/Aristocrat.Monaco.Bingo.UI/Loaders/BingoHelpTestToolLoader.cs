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

    public class BingoHelpTestToolLoader : IBingoPresentationLoader
    {
        private readonly IEventBus _eventBus;
        private readonly IBingoDisplayConfigurationProvider _configurationProvider;
        private readonly IDispatcher _dispatcher;
        private readonly IPropertiesManager _properties;

        public BingoHelpTestToolLoader(
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider configurationProvider,
            IDispatcher dispatcher,
            IPropertiesManager properties)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _configurationProvider =
                configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
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
                    new BingoHelpTestToolTab
                    {
                        ViewModel = new BingoHelpTestToolViewModel(_eventBus, _configurationProvider)
                    }));
        }
    }
}
#endif