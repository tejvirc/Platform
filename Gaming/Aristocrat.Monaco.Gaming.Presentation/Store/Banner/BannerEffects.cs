namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Services.IdleText;

public class BannerEffects
{
    private readonly IState<BannerState> _bannerState;
    private readonly IIdleTextService _idleTextService;

    public BannerEffects(IState<BannerState> bannerState, IIdleTextService idleTextService)
    {
        _bannerState = bannerState;
        _idleTextService = idleTextService;
    }

    [EffectMethod(typeof(PlatformDisabledAction))]
    public static async Task BannerPauseText(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new BannerPauseAction());
    }

    [EffectMethod(typeof(PlatformEnabledAction))]
    public static async Task BannerResumeText(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new BannerResumeAction());
    }

    [EffectMethod(typeof(StartupAction))]
    public async Task BannerStartup(IDispatcher dispatcher)
    {
        var defaultIdleText = _idleTextService.GetDefaultIdleText();
        await dispatcher.DispatchAsync(new BannerUpdateIdleTextAction(defaultIdleText));
    }
}