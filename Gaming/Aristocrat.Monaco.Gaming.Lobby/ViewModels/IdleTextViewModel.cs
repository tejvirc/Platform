namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Extensions.Fluxor;
using static Store.IdleText.IdleTextSelectors;

public class IdleTextViewModel : ObservableObject
{
    private readonly SubscriptionList _subscriptions = new();

    private string? _idleText;

    public IdleTextViewModel(ISelector selector)
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
