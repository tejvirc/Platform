namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;

public class PlatformButtonDeckViewModel : ObservableObject
{
    public PlatformButtonDeckViewModel()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
    }

    public RelayCommand LoadedCommand { get; }

    public string Title => GamingConstants.VbdWindowTitle;

    private void OnLoaded()
    {

    }
}
