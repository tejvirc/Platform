namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Common;
using Extensions.Fluxor;
using Microsoft.Extensions.Logging;
using Prism.Mvvm;
using static Store.Attract.AttractSelectors;

public class LobbyTopperViewModel : BindableBase
{
    private const string TopperImageDefaultResourceKey = "TopperBackground";
    private const string TopperImageAlternateResourceKey = "TopperBackgroundAlternate";

    private readonly ILogger<LobbyTopperViewModel> _logger;
    private readonly IStoreSelector _selector;

    private readonly SubscriptionList _subscriptions = new();

    private string? _topperImageResourceKey;

    public LobbyTopperViewModel(ILogger<LobbyTopperViewModel> logger, IStoreSelector selector, LobbyConfiguration configuration)
    {
        _logger = logger;
        _selector = selector;

        _subscriptions += selector.Select(SelectIsAlternateTopImageActive).Subscribe(
            active =>
            {
                TopperImageResourceKey = active ? TopperImageAlternateResourceKey : TopperImageDefaultResourceKey;
            });

        _subscriptions += selector.Select(SelectAttractModeTopperImageIndex).Subscribe(
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
