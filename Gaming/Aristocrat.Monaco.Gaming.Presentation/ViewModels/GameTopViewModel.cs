namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;
using Fluxor;
using Store;

public class GameTopViewModel : ObservableObject
{
    private readonly IDispatcher _dispatcher;

    private bool _isGameLoaded;

    public GameTopViewModel(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        GameWindowLoadedCommand = new RelayCommand<IntPtr>(OnGameWindowLoaded);
    }

    public string TopTitle => $"Game {GamingConstants.TopWindowTitle}";

    public bool IsGameLoaded
    {
        get => _isGameLoaded;

        set => SetProperty(ref _isGameLoaded, value);
    }

    public RelayCommand<IntPtr> GameWindowLoadedCommand { get; }

    private void OnGameWindowLoaded(IntPtr windowHandle)
    {
        _dispatcher.Dispatch(new GameTopWindowLoadedAction(windowHandle));
    }
}
