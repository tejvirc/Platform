namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluxor;
using Store;

public class GameTopperViewModel : ObservableObject
{
    private readonly IDispatcher _dispatcher;

    private bool _isActive;

    public GameTopperViewModel(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        LoadedCommand = new RelayCommand(OnLoaded);
        HostControlLoadedCommand = new RelayCommand<IntPtr>(OnHostControlLoaded);
    }

    public RelayCommand LoadedCommand { get; }

    public RelayCommand<IntPtr> HostControlLoadedCommand { get; }

    public string Title => "Game Topper";

    public bool IsActive
    {
        get => _isActive;

        set => SetProperty(ref _isActive, value);
    }

    private void OnLoaded()
    {

    }

    private void OnHostControlLoaded(IntPtr handle)
    {
        _dispatcher.Dispatch(new GameTopperWindowLoadedAction(handle));
    }
}
