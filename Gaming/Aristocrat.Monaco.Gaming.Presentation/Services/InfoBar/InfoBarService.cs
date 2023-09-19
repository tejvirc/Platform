namespace Aristocrat.Monaco.Gaming.Presentation.Services.InfoBar
{
    using Aristocrat.Cabinet.Contracts;
    using Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;
    using Fluxor;
    using Kernel;
    using Microsoft.Extensions.Logging;

    public class InfoBarService : IInfoBarService
    {
        private readonly ILogger<InfoBarService> _logger;
        private readonly IState<InfoBarState> _infoBarState;
        private readonly IPropertiesManager _properties;
        private readonly IEventBus _eventBus;
        public DisplayRole DisplayTarget { get; set; }

        public InfoBarService(
            IEventBus eventBus,
            ILogger<InfoBarService> logger,
            IState<InfoBarState> infoBarState,
            IPropertiesManager properties)
        {
            _eventBus = eventBus;
            _logger = logger;
            _infoBarState = infoBarState;
            _properties = properties;
        }

    }
}
