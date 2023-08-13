namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Views;
using Views.Events;

public class LobbyMainViewModel : BindableBase
{
    private readonly ILogger<LobbyMainViewModel> _logger;

    public LobbyMainViewModel(ILogger<LobbyMainViewModel> logger)
    {
        _logger = logger;

        LoadedCommand = new DelegateCommand(OnLoaded);
        RegionReadyCommand = new DelegateCommand<RegionReadyEventArgs>(OnRegionReady);
    }

    public DelegateCommand LoadedCommand { get; }

    public DelegateCommand<RegionReadyEventArgs> RegionReadyCommand { get; }

    private void OnLoaded()
    {
        //_regionManager.RequestNavigate(RegionNames.Chooser, ViewNames.Chooser);
        //_regionManager.RequestNavigate(RegionNames.Banner, ViewNames.Banner);
        //_regionManager.RequestNavigate(RegionNames.Upi, ViewNames.MultiLingualUpi);
        //_regionManager.RequestNavigate(RegionNames.InfoBar, ViewNames.InfoBar);
        //_regionManager.RequestNavigate(RegionNames.ReplayNav, ViewNames.ReplayNav);
    }

    private void OnRegionReady(RegionReadyEventArgs args)
    {
        switch(args.Region.Name)
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
