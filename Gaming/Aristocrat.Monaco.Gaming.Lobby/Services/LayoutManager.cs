namespace Aristocrat.Monaco.Gaming.Lobby.Services;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Aristocrat.Mgam.Client.Logging;
using Aristocrat.Monaco.Gaming.UI.Views.MediaDisplay;
using Aristocrat.Monaco.UI.Common;
using Hardware.Contracts.Cabinet;
using Kernel;
using log4net;
using ManagedBink;
using Regions;
using Toolkit.Mvvm.Extensions;
using Views;

public class LayoutManager : ILayoutManager
{
    private const string StatusWindowName = "StatusWindow";
    private const string ShellWindowName = "PlatformWindow";

    private readonly ILogger<LayoutManager> _logger;
    private readonly IPropertiesManager _properties;
    private readonly LobbyConfiguration _configuration;
    private readonly IWpfWindowLauncher _windowLauncher;
    private readonly ICabinetDetectionService _cabinetDetection;
    private readonly IRegionManager _regionManager;

    private readonly List<Window> _windows = new();
    private readonly List<ResourceDictionary> _skins = new();

    private Window? _shellWindow;

    public LayoutManager(
        ILogger<LayoutManager> logger,
        IPropertiesManager properties,
        LobbyConfiguration configuration,
        IWpfWindowLauncher windowLauncher,
        ICabinetDetectionService cabinetDetection,
        IRegionManager regionManager)
    {
        _logger = logger;
        _properties = properties;
        _configuration = configuration;
        _windowLauncher = windowLauncher;
        _cabinetDetection = cabinetDetection;
        _regionManager = regionManager;
    }

    public void CreateWindows()
    {
        Execute.OnUIThread(
            () =>
            {
                D3D.Init();

                CefHelper.Initialize();

                foreach (var skinFilename in _configuration.SkinFilenames)
                {
                    _skins.Add(SkinLoader.Load(skinFilename));
                }

                _regionManager.RegisterView<LobbyMainView>(RegionNames.Main, ViewNames.Lobby);
                _regionManager.RegisterView<AttractMainView>(RegionNames.Main, ViewNames.Attract);
                _regionManager.RegisterView<LoadingMainView>(RegionNames.Main, ViewNames.Loading);

                _regionManager.RegisterView<ChooserView>(RegionNames.Chooser, ViewNames.Chooser);

                _regionManager.RegisterView<StandardUpiView>(RegionNames.Upi, ViewNames.StandardUpi);
                _regionManager.RegisterView<StandardUpiView>(RegionNames.Upi, ViewNames.MultiLingualUpi);

                _regionManager.RegisterView<ReplayNavView>(RegionNames.ReplayNav, ViewNames.ReplayNav);

                _regionManager.RegisterView<InfoBarView>(RegionNames.InfoBar, ViewNames.InfoBar);

                _regionManager.RegisterView<PaidMeterView>(RegionNames.PaidMeter, ViewNames.PaidMeter);

                _regionManager.RegisterView<BannerView>(RegionNames.Banner, ViewNames.Banner);

                _regionManager.RegisterView<NotificationView>(RegionNames.Notification, ViewNames.Notification);

                _windowLauncher.Hide(StatusWindowName);

                _windowLauncher.CreateWindow<Shell>(ShellWindowName);

                _logger.LogDebug("Creating shell windows");
                _shellWindow = _windowLauncher.GetWindow(ShellWindowName);

                _shellWindow.Loaded += OnLoaded;

                //var mainGameWindow = new GameMain { Owner = shellWindow }
                //    .ShowWithTouch();

                //_windows.Add(mainGameWindow);

                //if (_cabinetDetection.Family == HardwareFamily.Unknown ||
                //    _cabinetDetection.GetDisplayDeviceByItsRole(DisplayRole.Top) != null)
                //{
                //    Logger.Debug("Creating top windows");
                //    var topWindow = new PlatformTop();
                //    topWindow.ShowWithTouch();
                //    var topGameWindow = new GameTop { Owner = topWindow };
                //    topGameWindow.ShowWithTouch();

                //    _windows.Add(topWindow);
                //    _windows.Add(topGameWindow);
                //}

                //if (_cabinetDetection.Family == HardwareFamily.Unknown &&
                //    _properties.GetValue("display", string.Empty) == "windowed" ||
                //    _cabinetDetection.GetDisplayDeviceByItsRole(DisplayRole.Topper) != null)
                //{
                //    Logger.Debug("Creating topper windows");
                //    var topperWindow = new PlatformTopper();
                //    topperWindow.ShowWithTouch();
                //    var topperGameWindow = new GameTopper { Owner = topperWindow };
                //    topperGameWindow.ShowWithTouch();

                //    _windows.Add(topperWindow);
                //    _windows.Add(topperGameWindow);
                //}

                //Logger.Debug("Creating button deck windows");
                //var buttonDeckWindow = new PlatformButtonDeck();
                //buttonDeckWindow.ShowWithTouch();
                //var buttonDeckGameWindow = new GameButtonDeck { Owner = buttonDeckWindow };
                //buttonDeckGameWindow.ShowWithTouch();

                //_windows.Add(buttonDeckWindow);
                //_windows.Add(buttonDeckGameWindow);

                _regionManager.NavigateToView(RegionNames.Main, ViewNames.Lobby);
                _regionManager.NavigateToView(RegionNames.Chooser, ViewNames.Chooser);
                _regionManager.NavigateToView(RegionNames.PaidMeter, ViewNames.PaidMeter);
                _regionManager.NavigateToView(RegionNames.Banner, ViewNames.Banner);
                _regionManager.NavigateToView(RegionNames.Notification, ViewNames.Notification);
                _regionManager.NavigateToView(RegionNames.InfoBar, ViewNames.InfoBar);
                _regionManager.NavigateToView(RegionNames.Upi, ViewNames.StandardUpi);
                _regionManager.NavigateToView(RegionNames.ReplayNav, ViewNames.ReplayNav);
            });
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
        tmpResource.MergedDictionaries.Add(_skins.First());

        window.Resources = tmpResource;
    }

    public void DestroyWindows()
    {
        Execute.OnUIThread(
            () =>
            {
                foreach (var window in _windows)
                {
                    window.Close();
                }

                _windowLauncher.Close(ShellWindowName);

                _windowLauncher.Show(StatusWindowName);
            });
    }
}
