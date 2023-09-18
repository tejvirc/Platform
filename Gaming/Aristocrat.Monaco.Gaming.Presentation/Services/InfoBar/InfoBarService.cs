namespace Aristocrat.Monaco.Gaming.Presentation.Services.InfoBar
{
    using Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar;
    using Fluxor;
    using Kernel;
    using Microsoft.Extensions.Logging;

    public class InfoBarService : IInfoBarService
    {
        private readonly ILogger<InfoBarService> _logger;
        private readonly IState<InfoBarState> _infoBarState;
        private readonly IPropertiesManager _properties;

        public InfoBarService(
            ILogger<InfoBarService> logger,
            IState<InfoBarState> infoBarState,
            IPropertiesManager properties)
        {
            _logger = logger;
            _infoBarState = infoBarState;
            _properties = properties;
        }
    }
}
