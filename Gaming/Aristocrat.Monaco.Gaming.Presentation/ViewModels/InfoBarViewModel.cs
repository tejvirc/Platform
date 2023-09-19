namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using Commands;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using Microsoft.Extensions.Logging;
using static Store.InfoBar.InfoBarSelectors;

public class InfoBarViewModel : ObservableObject, IActivatableViewModel
{
    public const double BarHeightFraction = 0.035;
    public const double BarHeightMinimum = 30;

    private readonly IDispatcher _dispatcher;
    private readonly ILogger<InfoBarViewModel> _logger;

    private bool _mainInfoBarOpenRequested;

    public InfoBarViewModel(
        ILogger<InfoBarViewModel> logger,
        IDispatcher dispatcher,
        IStore store,
        IApplicationCommands commands)
    {
        _dispatcher = dispatcher;
        _logger = logger;

        this.WhenActivated(
            disposables =>
            {
                store
                    .Select(SelectMainInfoBarOpenRequested)
                    .Subscribe(OnMainInfoBarOpenRequested)
                    .DisposeWith(disposables);
            });
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the Main InfoBar Open is Requested
    /// </summary>
    public bool MainInfoBarOpenRequested
    {
        get => _mainInfoBarOpenRequested;
        set => SetProperty(ref _mainInfoBarOpenRequested, value);
    }

    public ViewModelActivator Activator => new();

    private void OnMainInfoBarOpenRequested(bool mainInfoBarOpenRequested)
    {
        MainInfoBarOpenRequested = mainInfoBarOpenRequested;
    }
}