namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using Cabinet.Contracts;
using Contracts;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Views;

public class MainViewModel : BindableBase, INavigationAware
{
    private readonly IRegionManager _regionManager;

    public MainViewModel(IRegionManager regionManager)
    {
        _regionManager = regionManager;

        LoadedCommand = new DelegateCommand(OnLoaded);
    }

    public DelegateCommand LoadedCommand { get; }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
    }

    private void OnLoaded()
    {
        _regionManager.RequestNavigate(RegionNames.Main, ViewNames.Lobby);
    }
}
