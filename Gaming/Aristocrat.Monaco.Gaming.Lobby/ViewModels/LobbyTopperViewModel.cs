namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor.Extensions;
using Microsoft.Extensions.Logging;
using static Store.Lobby.LobbySelectors;

public class LobbyTopperViewModel : ObservableObject
{
    private const string TopperImageDefaultResourceKey = "TopperBackground";
    private const string TopperImageAlternateResourceKey = "TopperBackgroundAlternate";

    private readonly ILogger<LobbyTopperViewModel> _logger;
    private readonly ISelector _selector;

    private readonly SubscriptionList _subscriptions = new();

    private string? _topperImageResourceKey;

    public LobbyTopperViewModel(ILogger<LobbyTopperViewModel> logger, ISelector selector, LobbyConfiguration configuration)
    {
        _logger = logger;
        _selector = selector;

        _subscriptions += selector.Select(IsAlternateTopImageActiveSelector).Subscribe(
            active =>
            {
                TopperImageResourceKey = active ? TopperImageAlternateResourceKey : TopperImageDefaultResourceKey;
            });

        _subscriptions += selector.Select(AttractModeTopperImageIndexSelector).Subscribe(
            index =>
            {
                _logger.LogDebug(
                    "Setting Topper Image Index: {NewIndex} Resource ID: {ImageKey}",
                    index,
                    configuration.RotateTopperImageAfterAttractVideo[index]);

                TopperImageResourceKey = configuration.RotateTopperImageAfterAttractVideo[index];
            });
    }

    public string? TopperImageResourceKey
    {
        get => _topperImageResourceKey;

        set => SetProperty(ref _topperImageResourceKey, value);
    }
}
