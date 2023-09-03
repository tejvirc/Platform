namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluxor;
using Regions;
using Store.Bank;
using static Store.IdleText.IdleTextSelectors;

public class BannerViewModel : ObservableObject, INavigationAware
{
    private readonly IState<BankState> _bankState;

    public BannerViewModel(IStore store, IState<IdleTextState> bannerState, IState<BankState> bankState)
    {
        _bankState = bankState;

        store.Select(SelectIdleText)

        _bankState.Value.HasZeroCredits();

        LoadedCommand = new RelayCommand(OnLoaded);
        UnloadedCommand = new RelayCommand(OnUnloaded);
    }

    public RelayCommand LoadedCommand { get; }

    public RelayCommand UnloadedCommand { get; }

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
