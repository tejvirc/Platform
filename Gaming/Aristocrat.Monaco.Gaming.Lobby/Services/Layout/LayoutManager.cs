namespace Aristocrat.Monaco.Gaming.Lobby.Services.Layout;

using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Cabinet.Contracts;
using Fluxor;
using Hardware.Contracts.Cabinet;
using Kernel;
using log4net;
using Models;
using Regions;
using Store;
using Toolkit.Mvvm.Extensions;
using UI.Common;
using Views;

public class LayoutManager : ILayoutManager
{
    private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

    private const string StatusWindowName = "StatusWindow";
    private const string ShellWindowName = "PlatformWindow";

    private readonly IPropertiesManager _properties;
    private readonly IWpfWindowLauncher _windowLauncher;
    private readonly ICabinetDetectionService _cabinetDetection;
    private readonly IRegionManager _regionManager;

    private readonly List<Window> _windows = new();

    public LayoutManager(
        IPropertiesManager properties,
        IWpfWindowLauncher windowLauncher,
        ICabinetDetectionService cabinetDetection,
        IRegionManager regionManager)
    {
        _properties = properties;
        _windowLauncher = windowLauncher;
        _cabinetDetection = cabinetDetection;
        _regionManager = regionManager;
    }

    public void CreateWindows()
    {
        Execute.OnUIThread(
            () =>
            {
                _regionManager.RegisterView<LobbyMainView>(RegionNames.Main, ViewNames.Lobby);
                _regionManager.RegisterView<AttractMainView>(RegionNames.Main, ViewNames.Attract);
                _regionManager.RegisterView<LoadingMainView>(RegionNames.Main, ViewNames.Loading);

                _regionManager.RegisterView<ChooserView>(RegionNames.Chooser, ViewNames.Chooser);
                _regionManager.RegisterView<StandardUpiView>(RegionNames.Upi, ViewNames.StandardUpi);
                _regionManager.RegisterView<ReplayNavView>(RegionNames.ReplayNav, ViewNames.ReplayNav);
                _regionManager.RegisterView<InfoBarView>(RegionNames.InfoBar, ViewNames.InfoBar);
                _regionManager.RegisterView<PaidMeterView>(RegionNames.PaidMeter, ViewNames.PaidMeter);
                _regionManager.RegisterView<BannerView>(RegionNames.Banner, ViewNames.Banner);
                _regionManager.RegisterView<NotificationView>(RegionNames.Notification, ViewNames.Notification);

                _windowLauncher.Hide(StatusWindowName);

                _windowLauncher.CreateWindow<Shell>(ShellWindowName);

                Logger.Debug("Creating shell windows");
                var shellWindow = _windowLauncher.GetWindow(ShellWindowName);

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
            });
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
