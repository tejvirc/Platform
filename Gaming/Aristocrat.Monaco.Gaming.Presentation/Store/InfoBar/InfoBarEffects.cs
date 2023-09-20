namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar
{
    using Fluxor;
    using Services.InfoBar;

    public class InfoBarEffects
    {
        private readonly IState<InfoBarState> _infoBarState;
        private readonly IInfoBarService _infoBarService;

        public InfoBarEffects(IState<InfoBarState> infoBarState, IInfoBarService infoBarService)
        {
            _infoBarState = infoBarState;
            _infoBarService = infoBarService;
        }

    }
}
