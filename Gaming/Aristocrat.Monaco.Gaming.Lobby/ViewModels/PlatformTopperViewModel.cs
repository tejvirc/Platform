namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;

public class PlatformTopperViewModel : ObservableObject
{
    public PlatformTopperViewModel()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
    }

    public RelayCommand LoadedCommand { get; }

    public string Title => GamingConstants.TopperWindowTitle;

    private void OnLoaded()
    {

    }
}
