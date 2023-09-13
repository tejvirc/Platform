namespace Aristocrat.Monaco.Gaming.Presentation.Services.Translate;

using System;
using Application.Contracts;
using Common;
using Extensions.Fluxor;
using Gaming.Contracts;
using Kernel;
using static Store.Translate.TranslateSelectors;

public sealed class TranslateAgent : ITranslateAgent, IDisposable
{
    private static IStoreSelector? _selector;
    private readonly IPropertiesManager _properties;

    private readonly SubscriptionList _subscriptions = new();

    public string? ActiveLocaleCode = _selector?.Select(SelectActiveLocale).ToString();

    public TranslateAgent(IStoreSelector selector, IPropertiesManager properties)
    {
        _properties = properties;

        _subscriptions += selector.Select(SelectActiveLocale).Subscribe(code => ActiveLocaleCode = code);

        _selector = selector;
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public string GetSelectedLocaleCode()
    {
        return _properties.GetValue(GamingConstants.SelectedLocaleCode, GamingConstants.EnglishCultureCode)
            .ToUpperInvariant();
    }

    public void SetSelectedLocaleCode()
    {
        _properties.SetProperty(GamingConstants.SelectedLocaleCode, ActiveLocaleCode);
        _properties.SetProperty(ApplicationConstants.LocalizationPlayerCurrentCulture, ActiveLocaleCode);
    }
}