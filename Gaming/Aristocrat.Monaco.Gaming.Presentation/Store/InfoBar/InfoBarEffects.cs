namespace Aristocrat.Monaco.Gaming.Presentation.Store.InfoBar
{
    using System.Threading.Tasks;
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

        [EffectMethod]
        public Task Effect(InfoBarClearMessageAction action, IDispatcher dispatcher)
        {
            var tcs = new TaskCompletionSource<bool>();

            foreach (var region in action.Regions)
            {
                _infoBarService.ClearMessage(action.OwnerId, region);
            }

            if (string.IsNullOrWhiteSpace(_infoBarState.Value.LeftRegionText) &&
                string.IsNullOrWhiteSpace(_infoBarState.Value.CenterRegionText) &&
                string.IsNullOrWhiteSpace(_infoBarState.Value.RightRegionText))
            {
                dispatcher.Dispatch(new InfoBarCloseAction());
            }

            return tcs.Task;
        }
    }
}
