namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Common;
using Extensions.Fluxor;
using Prism.Mvvm;
using static Store.IdleText.IdleTextSelectors;

public class IdleTextViewModel : BindableBase
{
    private readonly SubscriptionList _subscriptions = new();

    private string? _idleText;

    public IdleTextViewModel(IStoreSelector selector)
    {
        _subscriptions += selector.Select(IdleTextSelector).Subscribe(
            text =>
            {
                IdleText = text;
            });
    }

    public string? IdleText
    {
        get => _idleText;

        set => SetProperty(ref _idleText, value);
    }
}
