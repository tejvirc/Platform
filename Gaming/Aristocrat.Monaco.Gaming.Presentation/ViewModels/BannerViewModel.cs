namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Regions;

public class BannerViewModel : ObservableObject, INavigationAware
{
    public BannerViewModel()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
        UnloadedCommand = new RelayCommand(OnUnloaded);
    }

    public RelayCommand LoadedCommand { get; }

    public RelayCommand UnloadedCommand { get; }

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
