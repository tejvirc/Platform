namespace Aristocrat.Monaco.Gaming.UI.Views.ButtonDeck
{
    using System;
    using System.Windows;
    using Cabinet.Contracts;
    using Contracts.Events.OperatorMenu;
    using Contracts.InfoBar;
    using Hardware.Contracts.Cabinet;
    using Kernel;
    using MahApps.Metro.Controls;
    using Toolkit.Mvvm.Extensions;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for VirtualButtonDeckOverlayView.xaml
    /// </summary>
    public partial class VirtualButtonDeckOverlayView
    {
        private readonly MetroWindow _parent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VirtualButtonDeckOverlayView" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public VirtualButtonDeckOverlayView(MetroWindow parent)
        {
            InitializeComponent();

            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;

            _parent = parent;

            // VLT-9338 : After a DisplayReconnectEvent in the vbd remap the overlay boundaries to parent(vbd), since the vbd
            // properties were moved upon disconnect. This has to be done after the DisplayReconnectEvent is handled in vbd
            // since it momentarily is behind the lobby until it is repositioned
            ServiceManager.GetInstance().GetService<IEventBus>().Subscribe<ResetVbdBoundariesEvent>(
                this,
                evt => Execute.OnUIThread(SetBoundaries));
        }

        /// <summary>
        ///     Gets or sets the view model
        /// </summary>
        public LobbyViewModel ViewModel
        {
            get => DataContext as LobbyViewModel;
            set => DataContext = value;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var isVisible = (bool)e.NewValue;
            if (isVisible)
            {
                SetBoundaries();
            }
        }

        private void SetBoundaries()
        {
            Top = _parent.Top;
            Left = _parent.Left;

            Width = _parent.Width;
            Height = _parent.Height;

            WindowState = _parent.WindowState;
            Cursor = _parent.Cursor;

            var visibleArea = ServiceManager.GetInstance().GetService<ICabinetDetectionService>()
                ?.GetDisplayDeviceByItsRole(DisplayRole.VBD)
                ?.VisibleArea;
            if (visibleArea != null)
            {
                var infoBarHeight = Math.Max(InfoBarViewModel.BarHeightMinimum, visibleArea.Value.Height * InfoBarViewModel.BarHeightFraction);
                ServiceManager.GetInstance().GetService<IEventBus>().Publish(new InfoBarSetHeightEvent(infoBarHeight, DisplayRole.VBD));
            }
        }

        private void OnClosed(object sender, EventArgs e)
        {
            IsVisibleChanged -= OnIsVisibleChanged;
            Closed -= OnClosed;

            ServiceManager.GetInstance().GetService<IEventBus>().UnsubscribeAll(this);
        }
    }
}