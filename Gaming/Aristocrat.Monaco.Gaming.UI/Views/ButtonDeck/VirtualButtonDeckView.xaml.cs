namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System;
    using System.Drawing;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Input;
    using Aristocrat.Monaco.Gaming.Contracts.Events.OperatorMenu;
    using Cabinet.Contracts;
    using Common;
    using Hardware.Contracts.Cabinet;
    using Hardware.Contracts.Display;
    using Kernel;
    using log4net;
    using ManagedBink;
    using Monaco.UI.Common;
    using MVVM;
    using ViewModels;
    using Cursors = System.Windows.Input.Cursors;

    /// <summary>
    ///     Interaction logic for VirtualButtonDeckView.xaml
    /// </summary>
    public partial class VirtualButtonDeckView
    {
        private readonly ICabinetDetectionService _cabinetDetectionService = ServiceManager.GetInstance().GetService<ICabinetDetectionService>();
        private readonly IDisplayDevice _vbdDisplayDevice;
        private readonly int _layoutRootHeight;
        private readonly int _layoutRootWidth;

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!.DeclaringType);

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualButtonDeckView" /> class.
        /// </summary>
        public VirtualButtonDeckView()
        {
            InitializeComponent();
            _vbdDisplayDevice = _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD);
            _layoutRootHeight = _vbdDisplayDevice?.Resolution.Y ?? 1080;
            _layoutRootWidth = _vbdDisplayDevice?.Resolution.X ?? 1920;

            ServiceManager.GetInstance().GetService<IEventBus>()
                .Subscribe<DisplayConnectedEvent>(this, evt => MvvmHelper.ExecuteOnUI(() => HandleEvent(evt)));
        }

        /// <summary>
        ///     Gets or sets the view model
        /// </summary>
        public LobbyViewModel ViewModel
        {
            get => DataContext as LobbyViewModel;
            set => DataContext = value;
        }

        private void WinHostCtrl_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.GameVirtualButtonDeckHwnd = GameVirtualButtonDeckWindowCtrl.Handle;
        }

        private void VirtualButtonDeckView_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateSize();
        }

        private void UpdateSize()
        {
            var properties = ServiceManager.GetInstance().GetService<IPropertiesManager>();
            var display = properties.GetValue(
                Constants.DisplayPropertyKey,
                Constants.DisplayPropertyFullScreen);
            var fullScreen = display == Constants.DisplayPropertyFullScreen;
            var vbdDisplayDevice = _cabinetDetectionService.GetDisplayDeviceByItsRole(DisplayRole.VBD);
            if (fullScreen)
            {
                var showMouseCursor = properties.GetValue(Constants.ShowMouseCursor, Constants.False);
                showMouseCursor = showMouseCursor.ToUpperInvariant();
                if (showMouseCursor == Constants.False)
                {
                    Cursor = Cursors.None;
                }
            }
            if (!fullScreen || vbdDisplayDevice == null)
            {
                SetWindowModeSize();
            }
            else
            {
                SetFullScreenModeSize();
            }
            Logger.Debug($"VBD window resized {Left},{Top},{Width},{Height}");
        }

        private void SetWindowModeSize()
        {
            ResizeMode = ResizeMode.CanResizeWithGrip;
            ShowTitleBar = true;
            ShowCloseButton = true;
            ShowMaxRestoreButton = true;
            ShowMinButton = true;
            Top = 0;
            Left = 0;
            Width = 1280;
            Height = 400;
        }

        private void SetFullScreenModeSize()
        {
            ShowTitleBar = false;
            ShowCloseButton = false;
            ShowMaxRestoreButton = false;
            ShowMinButton = false;
            Top = _vbdDisplayDevice.PositionY;
            Left = _vbdDisplayDevice.PositionX;
            Width = _vbdDisplayDevice.Resolution.X;
            Height = _vbdDisplayDevice.Resolution.Y;
            LayoutRoot.Height = _layoutRootHeight;
            LayoutRoot.Width = _layoutRootWidth;

            Logger.Debug(
                "SetFullScreenModeSize: "
                    + $"{new Rectangle((int)Top, (int)Left, (int)Width, (int)Height)} "
                    + $"LayoutRoot: {LayoutRoot.Width}x{LayoutRoot.Height} "
                    + $"Visible area: {new Rectangle(_vbdDisplayDevice.VisibleArea.XPos, _vbdDisplayDevice.VisibleArea.YPos, _vbdDisplayDevice.VisibleArea.Width, _vbdDisplayDevice.VisibleArea.Height)}");
        }

        private void VirtualButtonDeckView_OnClosed(object sender, EventArgs e)
        {
            TreeHelper.FindChild<BinkGpuControl>(this)?.Dispose();
            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
        }

        private void VbdContentControl_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            VbdMouseEvent(e);
        }

        private void VbdMouseEvent(MouseButtonEventArgs e)
        {
            // VLT-1797: While in Responsible Gaming Screen, any LCD button
            // press / VBD touch should exit to Chooser Screen.
            // VLT-2916 VLT-4370
            if (ViewModel.IsResponsibleGamingInfoDlgVisible && !ViewModel.IsTimeLimitDlgVisible &&
                !ViewModel.IsResponsibleGamingInfoFullScreen)
            {
                ViewModel.ExitResponsibleGamingInfoDialog();
                e.Handled = true;
            }

            ViewModel.OnUserInteraction();
        }

        private void HandleEvent(DisplayConnectedEvent @event)
        {
            Dispatcher?.BeginInvoke(
                new Action(
                    () =>
                    {
                        UpdateSize();
                        ServiceManager.GetInstance().GetService<IEventBus>().Publish(new ResetVbdBoundariesEvent());

                    }));
            // VLT-9338 : After a DisplayReconnectEvent in the vbd remap the overlay boundaries to parent(vbd), since the vbd
            // properties were moved upon disconnect. This has to be done after the DisplayReconnectEvent is handled in vbd
            // since it momentarily is behind the lobby until it is repositioned
        }
    }
}
