namespace Aristocrat.Monaco.Gaming.Presentation.ViewModels;

using System;
using Commands;
using CommunityToolkit.Mvvm.ComponentModel;
using Fluxor;
using Gaming.Contracts.InfoBar;
using Microsoft.Extensions.Logging;
using Store.InfoBar;
using static Store.InfoBar.InfoBarSelectors;

public class InfoBarViewModel : ObservableObject, IActivatableViewModel
{
    public const double BarHeightFraction = 0.035;
    public const double BarHeightMinimum = 30;
    private const double FontSizeMinimum = 16;
    private const double MarginSide = 4;
    private const double MarginTop = 6;

    private readonly IDispatcher _dispatcher;
    private readonly ILogger<InfoBarViewModel> _logger;

    private bool _mainInfoBarOpenRequested;
    private bool _isOpen;

    //private Thickness _margin;
    //private double _barHeight;
    //private double _fontSize = FontSizeMinimum;
    private string _leftRegionText;
    private string _centerRegionText;
    private string _rightRegionText;
    private double _leftRegionDuration;
    private double _centerRegionDuration;
    private double _rightRegionDuration;
    private InfoBarColor _backgroundColor;
    private InfoBarColor _leftRegionTextColor;
    private InfoBarColor _centerRegionTextColor;
    private InfoBarColor _rightRegionTextColor;

    public InfoBarViewModel(
        ILogger<InfoBarViewModel> logger,
        IDispatcher dispatcher,
        IStore store,
        IApplicationCommands commands,
        IState<InfoBarState> infoBarState)
    {
        _dispatcher = dispatcher;
        _logger = logger;

        this.WhenActivated(
            disposables =>
            {
                store
                    .Select(SelectMainInfoBarOpenRequested)
                    .Subscribe(code => MainInfoBarOpenRequested = code)
                    .DisposeWith(disposables);
                store
                    .Select(SelectInfoBarIsOpen)
                    .Subscribe(code => IsOpen = code)
                    .DisposeWith(disposables);
                store
                    .Select(SelectInfoBarLeftRegionDuration)
                    .Subscribe(duration => LeftRegionDuration = duration)
                    .DisposeWith(disposables);
                store
                    .Select(SelectInfoBarCenterRegionDuration)
                    .Subscribe(duration => CenterRegionDuration = duration)
                    .DisposeWith(disposables);
                store
                    .Select(SelectInfoBarRightRegionDuration)
                    .Subscribe(duration => _rightRegionDuration = duration)
                    .DisposeWith(disposables);
                store
                    .Select(SelectInfoBarLeftRegionTextColor)
                    .Subscribe(color => LeftRegionTextColor = color)
                    .DisposeWith(disposables);
                store
                    .Select(SelectInfoBarCenterRegionTextColor)
                    .Subscribe(color => CenterRegionTextColor = color)
                    .DisposeWith(disposables);
                store
                    .Select(SelectInfoBarRightRegionTextColor)
                    .Subscribe(color => RightRegionTextColor = color)
                    .DisposeWith(disposables);
            });
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the Main InfoBar Open is Requested
    /// </summary>
    public bool MainInfoBarOpenRequested
    {
        get => _mainInfoBarOpenRequested;
        set => SetProperty(ref _mainInfoBarOpenRequested, value);
    }

    public bool IsOpen
    {
        get => _isOpen;
        set => SetProperty(ref _isOpen, value);
    }

    public string LeftRegionText
    {
        get => _leftRegionText;
        set => SetProperty(ref _leftRegionText, value);
    }

    public string CenterRegionText
    {
        get => _centerRegionText;
        set => SetProperty(ref _centerRegionText, value);
    }

    public string RightRegionText
    {
        get => _rightRegionText;
        set => SetProperty(ref _rightRegionText, value);
    }

    public double LeftRegionDuration
    {
        get => _leftRegionDuration;
        set => SetProperty(ref _leftRegionDuration, value);
    }

    public double CenterRegionDuration
    {
        get => _centerRegionDuration;
        set => SetProperty(ref _centerRegionDuration, value);
    }

    public double RightRegionDuration
    {
        get => _rightRegionDuration;
        set => SetProperty(ref _rightRegionDuration, value);
    }

    public InfoBarColor BackgroundColor
    {
        get => _backgroundColor;
        set => SetProperty(ref _backgroundColor, value);
    }

    public InfoBarColor LeftRegionTextColor
    {
        get => _leftRegionTextColor;
        set => SetProperty(ref _leftRegionTextColor, value);
    }

    public InfoBarColor CenterRegionTextColor
    {
        get => _centerRegionTextColor;
        set => SetProperty(ref _centerRegionTextColor, value);
    }

    public InfoBarColor RightRegionTextColor
    {
        get => _rightRegionTextColor;
        set => SetProperty(ref _rightRegionTextColor, value);
    }

    public ViewModelActivator Activator => new();
}