namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using Prism.Commands;
using Prism.Mvvm;
using Regions;

public class BannerViewModel : BindableBase, INavigationAware
{
    public BannerViewModel()
    {
        LoadedCommand = new DelegateCommand(OnLoaded);
        UnloadedCommand = new DelegateCommand(OnUnloaded);
    }

    public DelegateCommand LoadedCommand { get; }

    public DelegateCommand UnloadedCommand { get; }

    private void OnLoaded()
    {
    }

    private void OnUnloaded()
    {
    }

    public void OnNavigateTo(NavigationContext context)
    {

    }
}
