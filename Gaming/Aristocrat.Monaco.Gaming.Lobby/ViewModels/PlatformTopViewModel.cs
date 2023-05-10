namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;

public class PlatformTopViewModel : ObservableObject
{
    public PlatformTopViewModel()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
    }

    public RelayCommand LoadedCommand { get; }

    public string Title => GamingConstants.TopWindowTitle;

    private void OnLoaded()
    {

    }
}
