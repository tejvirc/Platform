namespace Aristocrat.Monaco.Gaming.Presentation.Store.Translate;

using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Microsoft.Extensions.Options;
using Options;
using Services.Translate;

public class TranslateEffects
{
    private readonly IState<TranslateState> _translateState;
    private readonly TranslateOptions _translateOptions;
    private readonly ITranslateAgent _translateService;

    public TranslateEffects(
        IState<TranslateState> translateState,
        IOptions<TranslateOptions> translateOptions,
        ITranslateAgent translateService)
    {
        _translateState = translateState;
        _translateOptions = translateOptions.Value;
        _translateService = translateService;
    }

    [EffectMethod(typeof(StartupAction))]
    public async Task Startup(IDispatcher dispatcher)
    {
        await dispatcher.DispatchAsync(new TranslateUpdateMultiLanguageAction(_translateOptions.MultiLanguage));
        await dispatcher.DispatchAsync(new TranslateUpdateLocaleCodesAction(_translateOptions.LocaleCodes));

        if (_translateState.Value.IsMultiLangauge)
        {
            var localeCode = _translateService.GetSelectedLocaleCode();

            if (string.IsNullOrEmpty(localeCode) || _translateState.Value.LocaleCodes.Count == 1 ||
                localeCode == _translateState.Value.LocaleCodes.First().ToUpperInvariant())
            {
                _translateService.SetSelectedLocaleCode();
            }
            else
            {
                await dispatcher.DispatchAsync(new TranslateUpdateActiveLanguageAction(false));
            }
        }
        else
        {
            _translateService.SetSelectedLocaleCode();
        }
    }

    [EffectMethod(typeof(TranslateUpdateActiveLanguageAction))]
    public Task UpdateActiveLanguage(IDispatcher _)
    {
        if (_translateState.Value.IsMultiLangauge)
        {
            _translateService.SetSelectedLocaleCode();
        }

        return Task.CompletedTask;
    }
}
