namespace Aristocrat.Monaco.Gaming.Presentation.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
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
using Prism.Regions;
using Views;
using static Store.Translate.TranslateSelectors;

public sealed class LayoutManager : ILayoutManager, IDisposable
{
    private const string ShellWindowName = "ShellWindow";
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

                var shell = _windowLauncher.GetWindow(ShellWindowName);
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

        _windowLauncher.CreateWindow<Shell>(ShellWindowName);
        var shell = _windowLauncher.GetWindow(ShellWindowName);

        ChangeLanguageSkin(shell);

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

        //_regionManager.RequestNavigate(RegionNames.Shell, ViewNames.Main);
        //_regionManager.RequestNavigate(RegionNames.Main, ViewNames.Lobby);

        //_regionManager.NavigateToView(RegionNames.Main, ViewNames.Lobby);
        //_regionManager.NavigateToView(RegionNames.Chooser, ViewNames.Chooser);
        //_regionManager.NavigateToView(RegionNames.PaidMeter, ViewNames.PaidMeter);
        //_regionManager.NavigateToView(RegionNames.Banner, ViewNames.Banner);
        //_regionManager.NavigateToView(RegionNames.Notification, ViewNames.Notification);
        //_regionManager.NavigateToView(RegionNames.InfoBar, ViewNames.InfoBar);
        //_regionManager.NavigateToView(RegionNames.Upi, ViewNames.StandardUpi);
        //_regionManager.NavigateToView(RegionNames.ReplayNav, ViewNames.ReplayNav);
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

                    _windowLauncher.Close(ShellWindowName);

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
