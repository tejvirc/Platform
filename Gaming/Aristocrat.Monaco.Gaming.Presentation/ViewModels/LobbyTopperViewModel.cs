namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using static Store.Attract.AttractSelectors;

public class LobbyTopperViewModel : ObservableObject, IActivatableViewModel
{
    private const string TopperImageDefaultResourceKey = "TopperBackground";
    private const string TopperImageAlternateResourceKey = "TopperBackgroundAlternate";

    private readonly ILogger<LobbyTopperViewModel> _logger;

    private string? _topperImageResourceKey;

    public LobbyTopperViewModel(
        ILogger<LobbyTopperViewModel> logger,
        IStore store,
        IOptions<AttractOptions> attractOptions)
    {
        _logger = logger;

        this.WhenActivated(disposables =>
        {
            store
                .Select(SelectIsAlternateTopImageActive)
                .Subscribe(active =>
                {
                    TopperImageResourceKey = active ? TopperImageAlternateResourceKey : TopperImageDefaultResourceKey;
                })
                .DisposeWith(disposables);

            store
                .Select(SelectAttractModeTopperImageIndex)
                .Subscribe(index =>
                {
                    _logger.LogDebug(
                        "Setting Topper Image Index: {NewIndex} Resource ID: {ImageKey}",
                        index,
                        attractOptions.Value.TopperImageRotation[index]);

                    TopperImageResourceKey = attractOptions.Value.TopperImageRotation[index];
                })
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();

    public string? TopperImageResourceKey
    {
        get => _topperImageResourceKey;

        set => SetProperty(ref _topperImageResourceKey, value);
    }
}
