namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
using Aristocrat.Cabinet.Contracts;
using Aristocrat.MVVM;
using Common;
using Extensions.Fluxor;
using Hardware.Contracts.Cabinet;
using Kernel;
using ManagedBink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Monaco.UI.Common;
using Options;
using Prism.Ioc;
using Prism.Regions;
using Views;
using static Store.Translate.TranslateSelectors;

public sealed class LayoutManager : ILayoutManager, IDisposable
{
    private const string MainWindowName = "MainWindow";
    private const string StatusWindowName = "StatusWindow";

    private readonly ILogger<LayoutManager> _logger;
    private readonly IPropertiesManager _properties;
    private readonly PresentationOptions _presentationOptions;
    private readonly IWpfWindowLauncher _windowLauncher;
    private readonly ICabinetDetectionService _cabinetDetection;
    private readonly IRegionManager _regionManager;

    private readonly List<Window> _windows = new();
    private readonly List<ResourceDictionary> _skins = new();

    private readonly SubscriptionList _subscriptions = new();

    private int _activeSkinIndex;

    public LayoutManager(
        ILogger<LayoutManager> logger,
        IStoreSelector selector,
        IPropertiesManager properties,
        IOptions<PresentationOptions> presentationOptions,
        IWpfWindowLauncher windowLauncher,
        ICabinetDetectionService cabinetDetection,
        IRegionManager regionManager)
    {
        _logger = logger;
        _properties = properties;
        _presentationOptions = presentationOptions.Value;
        _windowLauncher = windowLauncher;
        _cabinetDetection = cabinetDetection;
        _regionManager = regionManager;

        _subscriptions += selector
            .Select(SelectPrimaryLanguageActive)
            .Select(isPrimary => isPrimary ? 0 : 1)
            .Subscribe(index =>
        {
            if (_activeSkinIndex != index)
            {
                _activeSkinIndex = index;

                var shell = _windowLauncher.GetWindow(MainWindowName);
                ChangeLanguageSkin(shell);

                foreach (var window in _windows)
                {
                    ChangeLanguageSkin(window);
                }
            }
        });
    }

    public Task InitializeAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        MvvmHelper.ExecuteOnUI(
            () =>
            {
                try
                {
                    InternalInitialize();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

        return tcs.Task;
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    private void InternalInitialize()
    {
        D3D.Init();

        foreach (var skinFilename in _presentationOptions.SkinFiles)
        {
            _skins.Add(SkinLoader.Load(skinFilename));
        }

        //_regionManager.RegisterView<LobbyMainView>(RegionNames.Main, ViewNames.Lobby);
        //_regionManager.RegisterView<AttractMainView>(RegionNames.Main, ViewNames.Attract);
        //_regionManager.RegisterView<LoadingMainView>(RegionNames.Main, ViewNames.Loading);

        //_regionManager.RegisterView<ChooserView>(RegionNames.Chooser, ViewNames.Chooser);

        //_regionManager.RegisterView<StandardUpiView>(RegionNames.Upi, ViewNames.StandardUpi);
        //_regionManager.RegisterView<StandardUpiView>(RegionNames.Upi, ViewNames.MultiLingualUpi);

        //_regionManager.RegisterView<ReplayNavView>(RegionNames.ReplayNav, ViewNames.ReplayNav);

        //_regionManager.RegisterView<InfoBarView>(RegionNames.InfoBar, ViewNames.InfoBar);

        //_regionManager.RegisterView<PaidMeterView>(RegionNames.PaidMeter, ViewNames.PaidMeter);

        //_regionManager.RegisterView<BannerView>(RegionNames.Banner, ViewNames.Banner);

        //_regionManager.RegisterView<NotificationView>(RegionNames.Notification, ViewNames.Notification);

        //_windowLauncher.CreateWindow<Shell>(ShellWindowName);

        //_logger.LogDebug("Creating shell windows");

        _windowLauncher.Hide(StatusWindowName);

        _windowLauncher.CreateWindow<Main>(MainWindowName);
        var mainWindow = _windowLauncher.GetWindow(MainWindowName);

        ChangeLanguageSkin(mainWindow);

        CreateGameWindow<GameMain>();

        CreateAndShowWindowWithTouch<ButtonDeck>();
        CreateGameWindow<GameButtonDeck>();

        if (_cabinetDetection.Family == HardwareFamily.Unknown ||
            _cabinetDetection.GetDisplayDeviceByItsRole(DisplayRole.Top) != null)
        {
            _logger.LogDebug("Creating top window");
            CreateAndShowWindow<Top>();
            CreateGameWindow<GameTop>();
        }

        if (_cabinetDetection.Family == HardwareFamily.Unknown &&
            _properties.GetValue("display", string.Empty) == "windowed" ||
            _cabinetDetection.GetDisplayDeviceByItsRole(DisplayRole.Topper) != null)
        {
            _logger.LogDebug("Creating topper window");
            CreateAndShowWindow<Topper>();
            CreateGameWindow<GameTopper>();
        }

        // _regionManager.RequestNavigate(RegionNames.Shell, ViewNames.Main);
        // _regionManager.RequestNavigate(RegionNames.Main, ViewNames.Lobby);

        //_regionManager.NavigateToView(RegionNames.Main, ViewNames.Lobby);
        //_regionManager.NavigateToView(RegionNames.Chooser, ViewNames.Chooser);
        //_regionManager.NavigateToView(RegionNames.PaidMeter, ViewNames.PaidMeter);
        //_regionManager.NavigateToView(RegionNames.Banner, ViewNames.Banner);
        //_regionManager.NavigateToView(RegionNames.Notification, ViewNames.Notification);
        //_regionManager.NavigateToView(RegionNames.InfoBar, ViewNames.InfoBar);
        //_regionManager.NavigateToView(RegionNames.Upi, ViewNames.StandardUpi);
        //_regionManager.NavigateToView(RegionNames.ReplayNav, ViewNames.ReplayNav);
    }

    private TWindow CreateWindow<TWindow>() where TWindow : Window
    {
        var window = ContainerLocator.Current.Resolve<TWindow>();

        _windows.Add(window);

        return window;
    }

    private void CreateGameWindow<TWindow>() where TWindow : Window
    {
        CreateWindow<TWindow>()
            .Show();
    }

    private void CreateAndShowWindow<TWindow>() where TWindow : Window
    {
        var window = CreateWindow<TWindow>();

        window.Loaded += OnLoaded;

        window.Show();
    }

    private void CreateAndShowWindowWithTouch<TWindow>() where TWindow : Window
    {
        var window = CreateWindow<TWindow>();

        window.Loaded += OnLoaded;

        window.ShowWithTouch();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Window window)
        {
            return;
        }

        ChangeLanguageSkin(window);
    }

    private void ChangeLanguageSkin(Window window)
    {
        var tmpResource = new ResourceDictionary();
        tmpResource.MergedDictionaries.Add(_skins.ElementAt(_activeSkinIndex));

        window.Resources = tmpResource;
    }

    public Task ShutdownAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        MvvmHelper.ExecuteOnUI(
            () =>
            {
                try
                {
                    foreach (var window in _windows)
                    {
                        window.Close();
                    }

                    _windowLauncher.Close(MainWindowName);

                    _windowLauncher.Show(StatusWindowName);

                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

        return tcs.Task;
    }
}
