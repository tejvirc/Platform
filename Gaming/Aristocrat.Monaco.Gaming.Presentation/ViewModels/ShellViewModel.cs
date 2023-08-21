namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Contracts;
using Events;
using Microsoft.Extensions.Logging;
using Prism.Common;
using Prism.Regions;
using Views;

public class ShellViewModel : ObservableObject
{
    private readonly ILogger<ShellViewModel> _logger;

    private ObservableObject<IRegion>? _mainRegion;

    public ShellViewModel(ILogger<ShellViewModel> logger)
    {
        _logger = logger;

        LoadedCommand = new RelayCommand(OnLoaded);
        RegionReadyCommand = new RelayCommand<RegionReadyEventArgs>(OnRegionReady);
    }

    public RelayCommand LoadedCommand { get; }

    public RelayCommand<RegionReadyEventArgs> RegionReadyCommand { get; }

    public ObservableObject<IRegion>? MainRegion
    {
        get => _mainRegion;

        set => SetProperty(ref _mainRegion, value);
    }

    public string MainTitle => GamingConstants.MainWindowTitle;

    private void OnLoaded()
    {
    }

    private void OnRegionReady(RegionReadyEventArgs? args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

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
