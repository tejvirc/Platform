namespace Aristocrat.Monaco.Gaming.Lobby.Store.Translate;

using System.Threading.Tasks;
using Services.Translate;
using Fluxor;
using Aristocrat.Monaco.Gaming.Contracts;

public class TranslateEffects
{
    private readonly ITranslateService _translateService;

    public TranslateEffects(ITranslateService translateService)
    {
        _translateService = translateService;
    }

    [EffectMethod]
    public async Task Effect(StartupAction action, IDispatcher dispatcher)
    {
        if (action.Configuration.MultiLanguageEnabled)
        {
            var localeCode = _translateService.GetSelectedLocaleCode();

            if (string.IsNullOrEmpty(localeCode) || action.Configuration.LocaleCodes.Length == 1 ||
                localeCode == action.Configuration.LocaleCodes[0].ToUpperInvariant())
            {
                await _translateService.SetSelectedLocaleCodeAsync();
            }
            else
            {
                await dispatcher.DispatchAsync(new UpdatePrimaryLanguageSelectedAction(false));
            }
        }
        else
        {
            _translateService.SetDefaultSelectedLocaleCode();
        }
    }
}
