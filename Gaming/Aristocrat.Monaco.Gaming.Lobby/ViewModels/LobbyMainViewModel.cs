namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using Cabinet.Contracts;
using Contracts;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Views;

public class LobbyMainViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;

    public LobbyMainViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;

        LoadedCommand = new DelegateCommand(OnLoaded);
    }

    public DelegateCommand LoadedCommand { get; }

    private void OnLoaded()
    {
        _regionManager.RequestNavigate(RegionNames.Chooser, ViewNames.Chooser);
        _regionManager.RequestNavigate(RegionNames.Banner, ViewNames.Banner);
        _regionManager.RequestNavigate(RegionNames.Upi, ViewNames.MultiLingualUpi);
        _regionManager.RequestNavigate(RegionNames.InfoBar, ViewNames.InfoBar);
        _regionManager.RequestNavigate(RegionNames.ReplayNav, ViewNames.ReplayNav);
    }
}
