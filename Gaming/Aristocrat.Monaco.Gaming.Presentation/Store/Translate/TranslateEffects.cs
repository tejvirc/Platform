namespace Aristocrat.Monaco.Gaming.Presentation.Store.Translate;

using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Extensions.Fluxor;
using Fluxor;
using Services.Translate;

public class TranslateEffects
{
    private readonly IState<TranslateState> _translateState;
    private readonly ITranslateAgent _translateService;

    public TranslateEffects(IState<TranslateState> translateState, ITranslateAgent translateService)
    {
        _translateState = translateState;
        _translateService = translateService;
    }

    [EffectMethod()]
    public async Task Effect(StartupAction _, IDispatcher dispatcher)
    {
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
                await dispatcher.DispatchAsync(new UpdateActiveLanguageAction(false));
            }
        }
        else
        {
            _translateService.SetSelectedLocaleCode();
        }
    }

    [EffectMethod()]
    public Task Effect(UpdateActiveLanguageAction action, IDispatcher dispatcher)
    {
        if (_translateState.Value.IsMultiLangauge)
        {
            _translateService.SetSelectedLocaleCode();
        }

        return Task.CompletedTask;
    }
}
