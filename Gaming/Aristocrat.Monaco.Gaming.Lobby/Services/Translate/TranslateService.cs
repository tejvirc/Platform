namespace Aristocrat.Monaco.Gaming.Lobby.Services.Translate;

using System.Reactive.Linq;
using System.Threading.Tasks;
using Contracts;
using Extensions.Fluxor;
using Kernel;
using static Store.Translate.TranslateSelectors;

public class TranslateService : ITranslateService
{
    private readonly ISelector _selector;
    private readonly IPropertiesManager _properties;

    public TranslateService(ISelector selector, IPropertiesManager properties)
    {
        _selector = selector;
        _properties = properties;
    }

    public string GetSelectedLocaleCode() =>
        _properties.GetValue(GamingConstants.SelectedLocaleCode, GamingConstants.EnglishCultureCode)
            .ToUpperInvariant();

    public async Task SetSelectedLocaleCodeAsync()
    {
        var activeLocaleCode = await _selector.Select(SelectActiveLocale).FirstAsync();

        _properties.SetProperty(GamingConstants.SelectedLocaleCode, activeLocaleCode);
    }

    public void SetDefaultSelectedLocaleCode()
    {
        _properties.SetProperty(GamingConstants.SelectedLocaleCode, GamingConstants.EnglishCultureCode);
    }
}
