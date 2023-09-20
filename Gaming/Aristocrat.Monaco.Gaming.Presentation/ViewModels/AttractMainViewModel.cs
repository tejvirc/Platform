﻿namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using Store.Attract;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using Kernel;
using Gaming.Contracts;
using Extensions.Fluxor;
using Store;
using System.Threading.Tasks;
using Services.Attract;
using static Store.Attract.AttractSelectors;
using Prism.Regions;
using Aristocrat.Monaco.Gaming.Presentation.Events;
using Aristocrat.Monaco.Gaming.Presentation.Views;
using CommunityToolkit.Mvvm.Input;

public class AttractMainViewModel : ObservableObject, IActivatableViewModel
{
    private readonly IState<AttractState> _attractState;
    private readonly IAttractService _attractService;
    private readonly IDispatcher _dispatcher;
    private readonly IStore _store;

    private IRegionManager? _regionManager;

    public AttractMainViewModel()
        : this(ServiceManager.GetInstance().TryGetService<IContainerService>().Container.GetInstance<IState<AttractState>>(),
              ServiceManager.GetInstance().TryGetService<IAttractService>(),
              ServiceManager.GetInstance().TryGetService<IContainerService>().Container.GetInstance<IDispatcher>(),
              ServiceManager.GetInstance().TryGetService<IContainerService>().Container.GetInstance<IStore>())
    { }

    public AttractMainViewModel(
        IState<AttractState> attractState,
        IAttractService attractService,
        IDispatcher dispatcher,
        IStore store)
    {
        _attractState = attractState;
        _attractService = attractService;
        _dispatcher = dispatcher;
        _store = store;

        RegionReadyCommand = new RelayCommand<RegionReadyEventArgs>(OnRegionReady);

        //this.WhenActivated(disposables =>
        //{
        //    store.Select(SelectAttractStarting)
        //    .WhenTrue()
        //    .Subscribe(index => _regionManager?.RequestNavigate(RegionNames.Attract, ViewNames.AttractMain))
        //    .DisposeWith(disposables);
        //});
    }

    public ViewModelActivator Activator => new();

    public RelayCommand<RegionReadyEventArgs> RegionReadyCommand { get; }

    public string BottomAttractVideoPath { get { return _attractState.Value.BottomVideoPath ?? ""; } }

    public bool IsBottomAttractVisible { get { return true || _attractState.Value.IsBottomPlaying; } }

    public bool IsBottomAttractVideoPlaying { get { return true || _attractState.Value.IsBottomPlaying; } }

    public void OnGameAttractVideoCompleted()
    {
        // Have to run this on a separate thread because we are triggering off an event from the video
        // and we end up making changes to the video control (loading new video).  The Bink Video Control gets very upset
        // if we try to do that on the same thread.

        Task.Run(() =>
        {
            MVVM.MvvmHelper.ExecuteOnUI(() =>
                _dispatcher.DispatchAsync(new AttractVideoCompletedAction())
            );
        });
    }

    private void OnRegionReady(RegionReadyEventArgs? args)
    {
        if (args == null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        _regionManager = (IRegionManager?)args.Region.RegionManager;

        switch (args.Region.Name)
        {
            case RegionNames.Attract:
                args.Handled = true;
                RequestNavigate(args.Region, ViewNames.AttractMain);
                break;
            default:
                //_logger.LogDebug("No handler found for {RegionName} region", args.Region.Name);
                break;
        }
    }

    private void RequestNavigate(IRegion region, string viewName)
    {
        region.RequestNavigate(viewName, (NavigationResult nr) =>
        {
            if (nr.Result != null && (bool)nr.Result)
            {
                return;
            }

            //_logger.LogError("Navigation failed for {View} into {Region}\n{Error}", ViewNames.Lobby, RegionNames.Main, nr.Error);
        });
    }
}
