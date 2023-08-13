namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using System.Reactive.Linq;
using Aristocrat.Extensions.Prism;
using Aristocrat.Monaco.Gaming.Lobby.Views.Events;
using Cabinet.Contracts;
using Contracts;
using Extensions.Fluxor;
using FMOD;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Common;
using Prism.Mvvm;
using Prism.Regions;
using Views;
using static Store.Lobby.LobbytSelectors;

public class ShellViewModel : BindableBase
{
    private readonly ILogger<ShellViewModel> _logger;

    private ObservableObject<IRegion>? _mainRegion;

    public ShellViewModel(ILogger<ShellViewModel> logger, IStoreSelector selector)
    {
        _logger = logger;

        LoadedCommand = new DelegateCommand(OnLoaded);
        RegionReadyCommand = new DelegateCommand<RegionReadyEventArgs>(OnRegionReady);
    }

    public DelegateCommand LoadedCommand { get; }

    public DelegateCommand<RegionReadyEventArgs> RegionReadyCommand { get; }

    public ObservableObject<IRegion>? MainRegion
    {
        get => _mainRegion;

        set => SetProperty(ref _mainRegion, value);
    }

    public string Title => GamingConstants.MainWindowTitle;

    public DisplayRole DisplayRole => DisplayRole.Main;

    private void OnLoaded()
    {
    }

    private void OnRegionReady(RegionReadyEventArgs args)
    {
        if (args.Region.Name != RegionNames.Main)
        {
            return;
        }

        args.Handled = true;

        RequestNavigate(args.Region, ViewNames.Lobby);
    }

    private void RequestNavigate(IRegion region, string viewName)
    {
        region.RequestNavigate(viewName, (NavigationResult nr) =>
        {
            if (nr.Result != null && (bool)nr.Result)
            {
                return;
            }

            _logger.LogError("Navigation failed for {View} into {Region}\n{Error}", ViewNames.Lobby, RegionNames.Main, nr.Error);
        });
    }
}
