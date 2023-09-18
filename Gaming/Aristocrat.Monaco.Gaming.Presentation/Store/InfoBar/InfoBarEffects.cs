namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar
{
    using System.Threading.Tasks;
    using Extensions.Fluxor;
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

        [EffectMethod(typeof(InfoBarRequestOpenAction))]
        public async Task ChangeGameControlHeight(IDispatcher dispatcher)
        {
            await dispatcher.DispatchAsync(new GameControlChangeHeightAction(0)); //TODO
        }
    }
}
