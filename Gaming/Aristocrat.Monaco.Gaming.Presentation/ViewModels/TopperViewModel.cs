namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Contracts;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Options;
using static Store.Attract.AttractSelectors;

public class TopperViewModel : ObservableObject, IActivatableViewModel
{
    private const string TopperImageDefaultResourceKey = "TopperBackground";
    private const string TopperImageAlternateResourceKey = "TopperBackgroundAlternate";

    private readonly ILogger<TopperViewModel> _logger;

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
                .Select(SelectIsAlternateTopperImageActive)
                .Subscribe(active =>
                {
                    TopperImageResourceKey = active ? TopperImageAlternateResourceKey : TopperImageDefaultResourceKey;
                })
                .DisposeWith(disposables);

            store
                .Select(SelectAttractModeTopperImageIndex)
                .Subscribe(index =>
                {
                    if (attractOptions.Value.TopperImageRotation is { Count: > 0})
                    {
                        _logger.LogDebug(
                            "Setting Topper Image Index: {NewIndex} Resource ID: {ImageKey}",
                            index,
                            configuration.RotateTopperImageAfterAttractVideo[index]);

                        TopperImageResourceKey = attractOptions.Value.TopperImageRotation[index];
                    }
                })
                .DisposeWith(disposables);
        });
    }

    public ViewModelActivator Activator { get; } = new();

    public string TopperTitle => GamingConstants.TopperWindowTitle;

    public string? TopperImageResourceKey
    {
        get => _topperImageResourceKey;

        set => SetProperty(ref _topperImageResourceKey, value);
    }
}
