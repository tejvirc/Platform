namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MVVM.Command;
using Options;
using Regions;
using Services.IdleText;
using Store;
using Store.Bank;
using Store.Chooser;
using static Store.Banner.BannerSelectors;

public class BannerViewModel : ObservableObject, INavigationAware, IActivatableViewModel
{
    private readonly IIdleTextService _idleTextService;
    private readonly IState<BankState> _bankState;
    private readonly IState<ChooserState> _chooserState;
    private readonly IDispatcher _dispatcher;
    private readonly IOptions<BannerOptions> _bannerOptions;
    private readonly IOptions<LobbyOptions> _lobbyOptions;
    private string? _idleText;
    private bool _isIdleTextPaused;
    private bool _isIdleTextScrolling;
    private bool _isScrollingDisplayMode;
    private string? _jurisdictionIdleText;

    public BannerViewModel(
        IStore store,
        IDispatcher dispatcher,
        IIdleTextService idleTextService,
        ILogger<BannerViewModel> logger,
        IState<BankState> bankState,
        IState<ChooserState> chooserState,
        IOptions<LobbyOptions> lobbyOptions,
        IOptions<BannerOptions> bannerOptions)
    {
        _bankState = bankState;
        _chooserState = chooserState;
        _dispatcher = dispatcher;
        _idleTextService = idleTextService;
        _bannerOptions = bannerOptions;
        _lobbyOptions = lobbyOptions;

        this.WhenActivated(
            disposables =>
            {
                store
                    .Select(CurrentIdleTextSelector)
                    .Subscribe(OnIdleTextUpdated)
                    .DisposeWith(disposables);
                store
                    .Select(IsPausedSelector)
                    .Subscribe(OnIdleTextPausedUpdated)
                    .DisposeWith(disposables);
                store
                    .Select(IsScrollingSelector)
                    .Subscribe(OnIdleTextScrollingUpdated)
                    .DisposeWith(disposables);
            });
        IdleTextScrollingCompletedCommand = new ActionCommand<object>(OnIdleTextScrollingCompleted);
    }

    /// <summary>
    ///     Idle text to show in lobby when user is not interacting with the game and not in attract mode
    /// </summary>
    public string? IdleText
    {
        get => _idleText;
        set => SetProperty(ref _idleText, value);
    }

    /// <summary>
    ///     Gets or sets the jurisdiction override idle text, from localized resource files
    /// </summary>
    public string JurisdictionIdleText
    {
        get => _jurisdictionIdleText;
        set
        {
            if (SetProperty(ref _jurisdictionIdleText, value))
            {
                _idleTextService.SetJurisdictionIdleText(value);
            }
        }
    }

    /// <summary>
    ///     Whether or not the text is too long and should scroll rather than blink
    /// </summary>
    public bool IsScrollingDisplayMode
    {
        get => _isScrollingDisplayMode;
        set
        {
            SetProperty(ref _isScrollingDisplayMode, value);
            OnPropertyChanged(nameof(IsIdleTextScrolling));
            OnPropertyChanged(nameof(IsBlinkingIdleTextVisible));
            OnPropertyChanged(nameof(IsScrollingIdleTextVisible));
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the idle text is paused or not
    /// </summary>
    public bool IsIdleTextPaused
    {
        get => _isIdleTextPaused;
        set => SetProperty(ref _isIdleTextPaused, value);
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the idle text is currently scrolling or not
    /// </summary>
    public bool IsIdleTextScrolling
    {
        get => _isIdleTextScrolling;
        set => SetProperty(ref _isIdleTextScrolling, value);
    }

    /// <summary>
    ///     Gets or sets action command that idle text scrolling completed event.
    /// </summary>
    public ICommand IdleTextScrollingCompletedCommand { get; set; }

    /// <summary>
    ///     Whether or not idle text should be shown (blinking version)
    /// </summary>
    public bool IsBlinkingIdleTextVisible => !IsScrollingDisplayMode &&
                                             (!_bannerOptions.Value.HideIdleTextOnCashIn ||
                                              _bankState.Value.HasZeroCredits()) && !_chooserState.Value.IsTabView &&
                                             !_lobbyOptions.Value.MidKnightTheme;

    /// <summary>
    ///     Whether or not idle text should be shown (scrolling version)
    /// </summary>
    public bool IsScrollingIdleTextVisible => IsScrollingDisplayMode &&
                                              (!_bannerOptions.Value.HideIdleTextOnCashIn ||
                                               _bankState.Value.HasZeroCredits()) && !_chooserState.Value.IsTabView &&
                                              !_lobbyOptions.Value.MidKnightTheme;

    /// <summary>
    ///     Activator
    /// </summary>
    public ViewModelActivator Activator { get; } = new();

    /// <summary>
    ///     OnNavigateTo
    /// </summary>
    /// <param name="context"></param>
    public void OnNavigateTo(NavigationContext context)
    {
    }

    private void OnIdleTextUpdated(string? text)
    {
        IdleText = text;
    }

    private void OnIdleTextPausedUpdated(bool isPaused)
    {
        IsIdleTextPaused = isPaused;
    }

    private void OnIdleTextScrollingUpdated(bool isScrolling)
    {
        IsIdleTextScrolling = isScrolling;
    }

    private void OnIdleTextScrollingCompleted(object obj)
    {
        _dispatcher.Dispatch(new BannerUpdateIsScrollingAction(false));
    }
}