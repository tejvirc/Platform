namespace Aristocrat.Monaco.Gaming.Presentation.Services.Translate;

using System;
using Common;
using Contracts;
using Extensions.Fluxor;
using Kernel;
using static Store.Translate.TranslateSelectors;

public sealed class TranslateAgent : ITranslateAgent, IDisposable
{
    private readonly IPropertiesManager _properties;

    private readonly SubscriptionList _subscriptions = new();

    private string? _activeLocaleCode;

    public TranslateAgent(IStoreSelector selector, IPropertiesManager properties)
    {
        _properties = properties;

        _subscriptions += selector.Select(SelectActiveLocale).Subscribe(code => _activeLocaleCode = code);
    }

    public string GetSelectedLocaleCode() =>
        _properties.GetValue(GamingConstants.SelectedLocaleCode, GamingConstants.EnglishCultureCode)
            .ToUpperInvariant();

    public void SetSelectedLocaleCode() =>
        _properties.SetProperty(GamingConstants.SelectedLocaleCode, _activeLocaleCode);

    public void Dispose()
    {
        _subscriptions.Dispose();
    }
}
