namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;
using Fluxor;
using Store;

public class GameTopperViewModel : ObservableObject
{
    private readonly IDispatcher _dispatcher;

    private bool _isGameLoaded;

    public GameTopperViewModel(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;

        GameWindowLoadedCommand = new RelayCommand<IntPtr>(OnGameWindowLoaded);
    }

    public string TopperTitle => $"Game {GamingConstants.TopperWindowTitle}";

    public bool IsGameLoaded
    {
        get => _isGameLoaded;

        set => SetProperty(ref _isGameLoaded, value);
    }

    public RelayCommand<IntPtr> GameWindowLoadedCommand { get; }

    private void OnGameWindowLoaded(IntPtr windowHandle)
    {
        _dispatcher.Dispatch(new GameTopperWindowLoadedAction(windowHandle));
    }
}
