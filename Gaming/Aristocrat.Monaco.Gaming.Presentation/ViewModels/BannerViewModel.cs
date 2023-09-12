namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluxor;
using Regions;
using static Store.IdleText.IdleTextSelectors;

public class BannerViewModel : ObservableObject, INavigationAware, IActivatableViewModel
{
    private string? _idleText;

    public BannerViewModel(IStore store)
    {
        this.WhenActivated(disposables =>
        {
            store
                .Select(SelectIdleText)
                .Subscribe(t => IdleText = t)
                .DisposeWith(disposables);
        });

        LoadedCommand = new RelayCommand(OnLoaded);
        UnloadedCommand = new RelayCommand(OnUnloaded);
    }

    public ViewModelActivator Activator { get; } = new();

    public RelayCommand LoadedCommand { get; }

    public RelayCommand UnloadedCommand { get; }

    public string? IdleText
    {
        get => _idleText;

        set => SetProperty(ref _idleText, value);
    }

    private void OnLoaded()
    {
    }

    private void OnUnloaded()
    {
    }

    public void OnNavigateTo(NavigationContext context)
    {

    }
}
