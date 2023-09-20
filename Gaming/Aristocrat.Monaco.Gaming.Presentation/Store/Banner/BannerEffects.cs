namespace Aristocrat.Monaco.Gaming.Presentation.Store.Banner;

using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Services.IdleText;

/// <summary>
///     Effects for the Banner component
/// </summary>
public class BannerEffects
{
    private readonly IState<BannerState> _bannerState;
    private readonly IIdleTextService _idleTextService;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="bannerState"></param>
    /// <param name="idleTextService"></param>
    public BannerEffects(IState<BannerState> bannerState, IIdleTextService idleTextService)
    {
        _bannerState = bannerState;
        _idleTextService = idleTextService;
    }

    /// <summary>
    ///     Pauses the banner text when platform is disabled
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <returns></returns>
    [EffectMethod(typeof(PlatformDisabledAction))]
    public static async Task BannerPauseText(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new BannerPauseAction());
    }

    /// <summary>
    ///     Resumes the banner text when platform is enabled
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <returns></returns>
    [EffectMethod(typeof(PlatformEnabledAction))]
    public static async Task BannerResumeText(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new BannerResumeAction());
    }

    /// <summary>
    ///     Initialized idle text fields from static properties or resources
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <returns></returns>
    [EffectMethod(typeof(StartupAction))]
    public async Task BannerStartup(IDispatcher dispatcher)
    {
        var defaultIdleText = _idleTextService.GetDefaultIdleText();
        var cabinetIdleText = _idleTextService.GetCabinetIdleText();
        await dispatcher.DispatchAsync(new BannerUpdateIdleTextAction(IdleTextType.Default, defaultIdleText));
        await dispatcher.DispatchAsync(new BannerUpdateIdleTextAction(IdleTextType.CabinetOrHost, cabinetIdleText));
    }
}