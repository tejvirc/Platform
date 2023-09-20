namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using System.Linq;
using Aristocrat.Monaco.Gaming.Contracts;
using Aristocrat.Monaco.Gaming.Presentation.Services.Attract;
using Aristocrat.Monaco.Kernel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Events;
using Fluxor;
using Microsoft.Extensions.Logging;
using Prism.Regions;
using Views;
using static Store.Attract.AttractSelectors;

public class LobbyMainViewModel : ObservableObject, IActivatableViewModel
{
    private readonly ILogger<LobbyMainViewModel> _logger;
    private IRegionManager? _regionManager;

    public LobbyMainViewModel(ILogger<LobbyMainViewModel> logger, IStore store)
    {
        _logger = logger;

        LoadedCommand = new RelayCommand(OnLoaded);
        RegionReadyCommand = new RelayCommand<RegionReadyEventArgs>(OnRegionReady);

        this.WhenActivated(disposables =>
        {
            store.Select(SelectAttractStarting)
                .WhenTrue()
                .Subscribe(index => _regionManager?.RequestNavigate(RegionNames.Attract, ViewNames.AttractMain))
                .DisposeWith(disposables);
        });
    }

    public RelayCommand LoadedCommand { get; }

    public RelayCommand<RegionReadyEventArgs> RegionReadyCommand { get; }

    public ViewModelActivator Activator => new();

    private void OnLoaded()
    {
        var _attractService = ServiceManager.GetInstance().TryGetService<IAttractService>();
        if (_attractService == null) _attractService = ServiceManager.GetInstance().TryGetService<IContainerService>().Container.GetInstance<IAttractService>();

        (_attractService as AttractService)?.StartAttractTimer();

        //_regionManager.RequestNavigate(RegionNames.Chooser, ViewNames.Chooser);
        //_regionManager.RequestNavigate(RegionNames.Banner, ViewNames.Banner);
        //_regionManager.RequestNavigate(RegionNames.Upi, ViewNames.MultiLingualUpi);
        //_regionManager.RequestNavigate(RegionNames.InfoBar, ViewNames.InfoBar);
        //_regionManager.RequestNavigate(RegionNames.ReplayNav, ViewNames.ReplayNav);
    }

    private void OnRegionReady(RegionReadyEventArgs? args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        _regionManager ??= args.Region.RegionManager;

        switch (args.Region.Name)
        {
            case RegionNames.Chooser:
                args.Handled = true;
                RequestNavigate(args.Region, ViewNames.Chooser);
                break;

            case RegionNames.Banner:
                args.Handled = true;
                RequestNavigate(args.Region, ViewNames.Banner);
                break;

            case RegionNames.Upi:
                args.Handled = true;
                RequestNavigate(args.Region, ViewNames.MultiLingualUpi);
                break;

            case RegionNames.InfoBar:
                args.Handled = true;
                RequestNavigate(args.Region, ViewNames.InfoBar);
                break;

            case RegionNames.ReplayNav:
                args.Handled = true;
                RequestNavigate(args.Region, ViewNames.ReplayNav);
                break;

            default:
                _logger.LogDebug("No handler found for {RegionName} region", args.Region.Name);
                break;
        };

    }

    private void RequestNavigate(IRegion region, string viewName)
    {
        region.RequestNavigate(viewName, (NavigationResult nr) =>
        {
            if (nr.Result != null && (bool)nr.Result)
            {
                return;
            }

            _logger.LogError("Navigation failed for {View} into {Region}\n{Error}", ViewNames.Lobby, RegionNames.Main, nr.Error);
        });
    }
}
