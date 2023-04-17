namespace Aristocrat.Monaco.Gaming.Lobby;

using System;
using System.Windows;
using Fluxor;
using Kernel;
using Models;
using Services;
using Store;
using Toolkit.Mvvm.Extensions;
using UI.Common;
using Views;

internal class LobbyLauncher : Contracts.Lobby.ILobby
{
    private const string StatusWindowName = "StatusWindow";

    private readonly IStore _store;
    private readonly IDispatcher _dispatcher;
    private readonly IPropertiesManager _properties;
    private readonly IWpfWindowLauncher _windowLauncher;
    private readonly IViewCollection _views;

    public LobbyLauncher(
        IStore store,
        IDispatcher dispatcher,
        IPropertiesManager properties,
        IWpfWindowLauncher windowLauncher,
        IViewCollection views)
    {
        _store = store;
        _dispatcher = dispatcher;
        _properties = properties;
        _windowLauncher = windowLauncher;
        _views = views;
    }

    public void CreateWindow()
    {
        RegisterViews();

        _store.InitializeAsync().Wait();

        _windowLauncher.Hide(StatusWindowName);

        Execute.OnUIThread(
            () =>
            {
                var windowType = _views.GetWindowViewType(ViewNames.Main);

                var mainWindow = (Window)Activator.CreateInstance(windowType) ??
                                 throw new InvalidOperationException($"Create window {windowType} failed");

                mainWindow.Show();
            });

        _dispatcher.Dispatch(new LoadGamesAction(LoadGameTrigger.OnDemand));
    }

    private void RegisterViews()
    {
        _views.RegisterWindowViewType<DefaultLobbyView>(ViewNames.Main);
        //_views.RegisterWindowViewType<GameMain>(ViewNames.Main);
        _views.RegisterWindowViewType<GameTop>(ViewNames.Top);
        _views.RegisterWindowViewType<GameTopper>(ViewNames.Topper);
        _views.RegisterWindowViewType<GameButtonDeck>(ViewNames.ButtonDeck);
        // _views.RegisterViewType<LobbyMainView>(ViewNames.LobbyMain);
        // _views.RegisterViewType<LobbyTopView>(ViewNames.LobbyTop);
        // _views.RegisterViewType<LobbyTopperView>(ViewNames.LobbyTopper);
        // _views.RegisterViewType<LobbyButtonDeckView>(ViewNames.LobbyButtonDeck);
        _views.RegisterViewType<ChooserView>(ViewNames.Chooser);
    }

    public void Show() => throw new NotSupportedException();

    public void Hide() => throw new NotSupportedException();

    public void Close()
    {
        _windowLauncher.Show(StatusWindowName);
        Execute.OnUIThread(
            () =>
            {
                var windowType = _views.GetWindowViewType(ViewNames.Main);

                var mainWindow = (Window)Activator.CreateInstance(windowType) ??
                                 throw new InvalidOperationException($"Create window {windowType} failed");

                mainWindow.Hide();
            });
    }
}
