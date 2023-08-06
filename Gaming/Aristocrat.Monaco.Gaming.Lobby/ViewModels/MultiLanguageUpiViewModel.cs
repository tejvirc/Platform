namespace Aristocrat.Monaco.Gaming.Lobby.ViewModels;

using System;
using Application.Contracts.Extensions;
using Commands;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Extensions.Fluxor;
using Fluxor;
using Store;
using static Store.Bank.BankSelectors;
using static Store.Chooser.ChooserSelectors;
using static Store.Translate.TranslateSelectors;

public class MultiLanguageUpiViewModel : ObservableObject
{
    private readonly IDispatcher _dispatcher;
    private readonly LobbyConfiguration _configuration;

    private readonly SubscriptionList _subscriptions = new();

    private string? _activeLocaleCode;
    private string? _formattedCredits;
    private bool _cashOutEnabled;
    private bool _isPrimaryLanguageSelected;
    private int _denomFilter;
    private string? _languageButtonResourceKey;

    public MultiLanguageUpiViewModel(
        IDispatcher dispatcher,
        IStoreSelector selector,
        IApplicationCommands commands,
        LobbyConfiguration configuration)
    {
        _dispatcher = dispatcher;
        _configuration = configuration;

        ShutdownCommand = new RelayCommand(OnShutdown);
        CashOutCommand = new RelayCommand(OnCashOut);
        CashOutWrapperMouseDownCommand = new RelayCommand(OnCashOutWrapperMouseDown);
        UpiPreviewMouseDownCommand = new RelayCommand(OnUpiPreviewMouseDown);
        DenomFilterPressedCommand = new RelayCommand(OnDenomFilterPressed);
        UserInteractionCommand = new RelayCommand(OnUserInteraction);
        ToggleActiveLangaugeCommand = new RelayCommand(OnToggleActiveLangauge);

        commands.ShutdownCommand.RegisterCommand(ShutdownCommand);

        _subscriptions += selector.Select(SelectPrimaryLanguageActive).Subscribe(OnPrimaryLanguageActiveChanged);
        _subscriptions += selector.Select(SelectActiveLocale).Subscribe(OnActiveLocaleChanged);
        _subscriptions += selector.Select(SelectDenomFilter).Subscribe(OnDenomFilterChanged);
        _subscriptions += selector.Select(SelectCredits).Subscribe(OnCreditsChanged);
    }

    public RelayCommand ShutdownCommand { get; }

    public RelayCommand CashOutWrapperMouseDownCommand { get; }

    public RelayCommand CashOutCommand { get; }

    public RelayCommand UpiPreviewMouseDownCommand { get; }

    public RelayCommand DenomFilterPressedCommand { get; }

    public RelayCommand UserInteractionCommand { get; }

    public RelayCommand ToggleActiveLangaugeCommand { get; }

    public string? ActiveLocaleCode
    {
        get => _activeLocaleCode;

        set => SetProperty(ref _activeLocaleCode, value);
    }

    public string? FormattedCredits
    {
        get => _formattedCredits;

        set => SetProperty(ref _formattedCredits, value);
    }

    public bool CashOutEnabled
    {
        get => _cashOutEnabled;

        set => SetProperty(ref _cashOutEnabled, value);
    }

    public bool IsPrimaryLanguageSelected
    {
        get => _isPrimaryLanguageSelected;

        set => SetProperty(ref _isPrimaryLanguageSelected, value);
    }

    public int DenomFilter
    {
        get => _denomFilter;

        set => SetProperty(ref _denomFilter, value);
    }

    public string? LanguageButtonResourceKey
    {
        get => _languageButtonResourceKey;

        set => SetProperty(ref _languageButtonResourceKey, value);
    }

    private void OnShutdown()
    {
        _subscriptions.UnsubscribeAll();
    }

    private void OnCashOut()
    {
    }

    private void OnCashOutWrapperMouseDown()
    {
    }

    private void OnUpiPreviewMouseDown()
    {
        //if (IsResponsibleGamingInfoDlgVisible && !IsResponsibleGamingInfoFullScreen)
        //{
        //    ExitResponsibleGamingInfoDialog();
        //    OnUserInteraction();
        //    if (obj is MouseButtonEventArgs e)
        //    {
        //        e.Handled = true;
        //    }
        //}
    }

    private void OnDenomFilterPressed()
    {
        _dispatcher.Dispatch(new UpdateDenomFilterAction(DenomFilter));
        // OnUserInteraction();
    }

    private void OnUserInteraction()
    {
        //Logger.Debug($"OnUserInteraction, state: {CurrentState}");

        //_eventBus.Publish(new UserInteractionEvent());

        //// Reset idle timer when user interacted with lobby.
        //if (_idleTimer != null && _idleTimer.IsEnabled)
        //{
        //    _idleTimer.Stop();
        //    _idleTimer.Start();
        //}

        //ExitAndResetAttractMode();

        //_lobbyStateManager.OnUserInteraction();
        //SetEdgeLighting();
    }

    private void OnToggleActiveLangauge()
    {
        _dispatcher.Dispatch(new UpdateActiveLanguageAction(IsPrimaryLanguageSelected));
    }

    private void OnActiveLocaleChanged(string activeLocaleCode)
    {
        ActiveLocaleCode = activeLocaleCode;
    }

    private void OnPrimaryLanguageActiveChanged(bool isPrimaryLanguageActive)
    {
        IsPrimaryLanguageSelected = isPrimaryLanguageActive;
        LanguageButtonResourceKey = _configuration.LanguageButtonResourceKeys[isPrimaryLanguageActive ? 1 : 0];
    }

    private void OnDenomFilterChanged(int denomFilter)
    {
        DenomFilter = denomFilter;
    }

    private void OnCreditsChanged(double credits)
    {
        FormattedCredits = credits.FormattedCurrencyString();
    }
}
