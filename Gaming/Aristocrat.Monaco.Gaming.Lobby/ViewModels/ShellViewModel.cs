namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using Cabinet.Contracts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;

public class ShellViewModel : ObservableObject
{
    public ShellViewModel()
    {
        LoadedCommand = new RelayCommand(OnLoaded);
    }

    public RelayCommand LoadedCommand { get; }

    public string Title => GamingConstants.MainWindowTitle;

    public DisplayRole DisplayRole => DisplayRole.Main;

    private void OnLoaded()
    {

    }
}
