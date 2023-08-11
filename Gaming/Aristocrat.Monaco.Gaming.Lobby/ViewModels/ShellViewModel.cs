namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using System.Reactive.Linq;
using Cabinet.Contracts;
using Contracts;
using Extensions.Fluxor;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using ReactiveUI;
using Views;
using static Store.Lobby.LobbytSelectors;

public class ShellViewModel : BindableBase
{
    private readonly IRegionManager _regionManager;

    public ShellViewModel(IStoreSelector selector, IRegionManager regionManager)
    {
        _regionManager = regionManager;

        LoadedCommand = new DelegateCommand(OnLoaded);
        RegionLoadedCommand = new DelegateCommand(OnRegionLoaded);

        selector.Select(SelectLobbyInitailized).Where(initialized => initialized).Subscribe(OnLobbyInitialized);
    }

    public DelegateCommand LoadedCommand { get; }

    public DelegateCommand RegionLoadedCommand { get; }

    public string Title => GamingConstants.MainWindowTitle;

    public DisplayRole DisplayRole => DisplayRole.Main;

    private void OnLoaded()
    {
    }

    private void OnRegionLoaded()
    {
    }

    private void OnLobbyInitialized(bool isInitialize)
    {
        //    _regionManager.RequestNavigate(RegionNames.Main, new Uri(ViewNames.Lobby, UriKind.Relative), (NavigationResult nr) =>
        //    {
        //        var error = nr.Error;
        //        var result = nr.Result;
        //    });
    }
}
