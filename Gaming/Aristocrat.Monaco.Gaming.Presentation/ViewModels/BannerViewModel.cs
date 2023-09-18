namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Fluxor;
using log4net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monaco.UI.Common;
using MVVM.Command;
using Options;
using Regions;
using Store;
using Store.Bank;
using Store.Chooser;
using static Store.Banner.BannerSelectors;
using static Store.Translate.TranslateSelectors;

public class BannerViewModel : ObservableObject, INavigationAware, IActivatableViewModel
{
    private const double MaximumBlinkingIdleTextWidth = 500;
    private const string IdleTextFamilyName = "Segoe UI";
    private const int IdleTextFontSize = 32;
    private readonly IState<BankState> _bankState;
    private readonly IState<ChooserState> _chooserState;
    private readonly IDispatcher _dispatcher;
    private readonly IOptions<BannerOptions> _bannerOptions;
    private readonly IOptions<LobbyOptions> _lobbyOptions;
    private string? _idleText;
    private bool _isIdleTextShowing = true;
    private bool _isIdleTextPaused;
    private bool _isIdleTextScrolling;
    private bool _isScrollingDisplayMode;
    private bool _useDefaultIdleText = true;

    //#TODO: REMOVE THESE ONCE ADDRESSED PROPERLY:
    //#TODO: Get disabled state from Lobby
    private readonly bool IsInStateLobbyStateDisabled = false;

    //#TODO: Still need IsInLobby or will that always be true here?
    private readonly bool IsInLobby = true;

    public BannerViewModel(
        IStore store,
        IDispatcher dispatcher,
        ILogger<BannerViewModel> logger,
        IState<BankState> bankState,
        IState<ChooserState> chooserState,
        IOptions<LobbyOptions> lobbyOptions,
        IOptions<BannerOptions> bannerOptions)
    {
        _bankState = bankState;
        _chooserState = chooserState;
        _dispatcher = dispatcher;
        _bannerOptions = bannerOptions;
        _lobbyOptions = lobbyOptions;

        this.WhenActivated(
            disposables =>
            {
                store
                    .Select(IdleTextSelector)
                    .Subscribe(OnIdleTextUpdated)
                    .DisposeWith(disposables);
                store
                    .Select(IsPausedSelector)
                    .Subscribe(OnIdleTextPausedUpdated)
                    .DisposeWith(disposables);
                store
                    .Select(IsIdleTextShowingSelector)
                    .Subscribe(OnIdleTextShowingUpdated)
                    .DisposeWith(disposables);
                store
                    .Select(IsScrollingSelector)
                    .Subscribe(OnIdleTextScrollingUpdated)
                    .DisposeWith(disposables);
                store
                    .Select(SelectActiveLocale)
                    .Subscribe(code => OnLanguageChanged(code))
                    .DisposeWith(disposables);
            });
        IdleTextScrollingCompletedCommand = new ActionCommand<object>(OnIdleTextScrollingCompleted);
        LoadedCommand = new RelayCommand(OnLoaded);
        UnloadedCommand = new RelayCommand(OnUnloaded);

    }

    public RelayCommand LoadedCommand { get; }
    public RelayCommand UnloadedCommand { get; }

    private void OnLoaded()
    {
        // DENNIS: TEMP: Just trying to see if string can be loaded here...this works if resource added to App
        string txt = (string)Application.Current.TryFindResource("LobbyIdleTextDefault");
        if (txt == null)
        {

        }
    }

    private void OnUnloaded()
    {
    }

    public string? IdleText
    {
        get => _idleText;
        set
        {
            if (SetProperty(ref _idleText, value))
            {
                IsScrollingDisplayMode = ShouldIdleTextScroll(_idleText);
            }

            OnPropertyChanged(nameof(IsIdleTextScrolling));
            OnPropertyChanged(nameof(IsBlinkingIdleTextVisible));
            OnPropertyChanged(nameof(IsScrollingIdleTextEnabled));
            OnPropertyChanged(nameof(IsScrollingIdleTextVisible));
        }
    }

    public bool IsScrollingDisplayMode
    {
        get => _isScrollingDisplayMode;
        set
        {
            SetProperty(ref _isScrollingDisplayMode, value);
            OnPropertyChanged(nameof(IsIdleTextScrolling));
            OnPropertyChanged(nameof(IsBlinkingIdleTextVisible));
            OnPropertyChanged(nameof(IsScrollingIdleTextEnabled));
            OnPropertyChanged(nameof(IsScrollingIdleTextVisible));
        }
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the idle text is currently visible or not
    /// </summary>
    public bool IsIdleTextShowing
    {
        get => _isIdleTextShowing;
        set => SetProperty(ref _isIdleTextShowing, value);
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
    ///     Gets or sets a value indicating whether to use default (localized) idle text or text
    ///     provided by the service
    /// </summary>
    public bool UseDefaultIdleText
    {
        get => _useDefaultIdleText;
        set => SetProperty(ref _useDefaultIdleText, value);
    }

    /// <summary>
    ///     Gets or sets action command that idle text scrolling completed event.
    /// </summary>
    public ICommand IdleTextScrollingCompletedCommand { get; set; }

    public bool IsBlinkingIdleTextVisible => !IsScrollingDisplayMode &&
                                             (!_bannerOptions.Value.HideIdleTextOnCashIn ||
                                              _bankState.Value.HasZeroCredits()) && !_chooserState.Value.IsTabView &&
                                             !_lobbyOptions.Value.MidKnightTheme;

    public bool IsScrollingIdleTextEnabled => IsScrollingDisplayMode &&
                                              (!_bannerOptions.Value.HideIdleTextOnCashIn ||
                                               _bankState.Value.HasZeroCredits()) && !_chooserState.Value.IsTabView &&
                                              !_lobbyOptions.Value.MidKnightTheme;

    public bool IsScrollingIdleTextVisible => IsScrollingIdleTextEnabled;

    public bool IsIdleTextBlinking => IsInLobby && !IsInStateLobbyStateDisabled;

    public bool StartIdleTextBlinking => IsBlinkingIdleTextVisible && IsIdleTextBlinking;

    public ViewModelActivator Activator { get; } = new();

    public void OnNavigateTo(NavigationContext context)
    {
    }

    private static bool ShouldIdleTextScroll(string? idleText)
    {
        if (string.IsNullOrEmpty(idleText))
        {
            return false;
        }

        var formattedText = new FormattedText(
            idleText,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily(IdleTextFamilyName), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal),
            IdleTextFontSize,
            Brushes.Black,
            new NumberSubstitution(),
            1);

        return new Size(formattedText.Width, formattedText.Height).Width > MaximumBlinkingIdleTextWidth;
    }

    private void OnIdleTextUpdated(string? text)
    {
        IdleText = text;
    }

    private void OnIdleTextPausedUpdated(bool isPaused)
    {
        IsIdleTextPaused = isPaused;
    }

    private void OnIdleTextShowingUpdated(bool isShowing)
    {
        IsIdleTextShowing = isShowing;
    }

    private void OnIdleTextScrollingUpdated(bool isScrolling)
    {
        IsIdleTextScrolling = isScrolling;
    }

    private void OnIdleTextScrollingCompleted(object obj)
    {
        _dispatcher.Dispatch(new BannerUpdateIsScrollingAction(false));
    }

    private void OnLanguageChanged(string code)
    {
        //#TODO: Handle language change to get new idle text, either here or somewhere else.
    }
}