namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Events;
using Microsoft.Extensions.Logging;
using Prism.Regions;
using Views;

public class LobbyMainViewModel : ObservableObject
{
    private readonly ILogger<LobbyMainViewModel> _logger;

    private IRegionManager? _regionManager;

    public LobbyMainViewModel(ILogger<LobbyMainViewModel> logger)
    {
        _logger = logger;

        LoadedCommand = new RelayCommand(OnLoaded);
        RegionReadyCommand = new RelayCommand<RegionReadyEventArgs>(args => OnRegionReady(args).Wait());
    }

    public RelayCommand LoadedCommand { get; }

    public RelayCommand<RegionReadyEventArgs> RegionReadyCommand { get; }

    private void OnLoaded()
    {
        // _regionManager.RequestNavigate(RegionNames.Chooser, ViewNames.Chooser);
        // _regionManager.RequestNavigate(RegionNames.Banner, ViewNames.Banner);
        // _regionManager.RequestNavigate(RegionNames.Upi, ViewNames.MultiLingualUpi);
        // _regionManager.RequestNavigate(RegionNames.InfoBar, ViewNames.InfoBar);
        // _regionManager.RequestNavigate(RegionNames.ReplayNav, ViewNames.ReplayNav);
    }

    private async Task OnRegionReady(RegionReadyEventArgs? args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        _regionManager ??= args.Region.RegionManager;

        switch(args.Region.Name)
        {
            case RegionNames.Chooser:
                args.Handled = true;
                await args.Region.RequestNavigateAsync(ViewNames.Chooser);
                break;

            case RegionNames.Banner:
                args.Handled = true;
                await args.Region.RequestNavigateAsync(ViewNames.Banner);
                break;

            case RegionNames.Upi:
                args.Handled = true;
                await args.Region.RequestNavigateAsync(ViewNames.MultiLingualUpi);
                break;

            case RegionNames.InfoBar:
                args.Handled = true;
                await args.Region.RequestNavigateAsync(ViewNames.InfoBar);
                break;

            case RegionNames.ReplayNav:
                args.Handled = true;
                await args.Region.RequestNavigateAsync(ViewNames.ReplayNav);
                break;

            case RegionNames.PaidMeter:
                args.Handled = true;
                await args.Region.RequestNavigateAsync(ViewNames.PaidMeter);
                break;

            case RegionNames.Notification:
                args.Handled = true;
                await args.Region.RequestNavigateAsync(ViewNames.Notification);
                break;

            default:
                _logger.LogDebug("No handler found for {RegionName} region", args.Region.Name);
                break;
        }
    }
}
