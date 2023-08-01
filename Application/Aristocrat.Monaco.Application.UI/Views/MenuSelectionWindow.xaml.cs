namespace Aristocrat.Monaco.Application.UI.Views
{
    using System;
    using System.Windows;
    using Cabinet.Contracts;
    using Monaco.Common;
    using ViewModels;

    /// <summary>
    ///     Interaction logic for MenuSelectionWindow.xaml
    /// </summary>
    [CLSCompliant(false)]
    public sealed partial class MenuSelectionWindow : IDisposable
    {
        private readonly WindowToScreenMapper _screenMapper = new WindowToScreenMapper(DisplayRole.Main);
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuSelectionWindow" /> class.
        /// </summary>
        public MenuSelectionWindow()
        {
            ViewModel = new MenuSelectionViewModel();

            InitializeComponent();

            ViewModel.LoadFinished += ViewModel_LoadFinished;
            ViewModel.DisplayChanged += ViewModel_DisplayChanged;
            Topmost = ViewModel.TopMost;

            ContentElement = MenuContentControl;
        }

        /// <summary>
        ///     Gets or sets the view model.
        /// </summary>
        /// <value>
        ///     The view model.
        /// </value>
        public MenuSelectionViewModel ViewModel
        {
            get => DataContext as MenuSelectionViewModel;
            set => DataContext = value;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            ViewModel.LoadFinished -= ViewModel_LoadFinished;
            ViewModel.DisplayChanged -= ViewModel_DisplayChanged;
            ViewModel.Dispose();

            _disposed = true;
        }

        private void ViewModel_LoadFinished(object sender, EventArgs eventArgs)
        {
            Activate();
            Focus();

            RestoreWindowPlacement();
        }

        private void ViewModel_DisplayChanged(object sender, EventArgs e)
        {
            Dispatcher?.InvokeAsync(RestoreWindowPlacement);
        }

        /// <summary>
        ///     Handles initializing member variables when the window is initialized.
        /// </summary>
        /// <param name="sender">The sender of the event. Not used.</param>
        /// <param name="e">Parameters for the event. Not used.</param>
        private void Window_Initialized(object sender, EventArgs e)
        {
            ViewModel.SetupProperties();

            // Workaround solution to eliminate the flash effect when a WPF window is newly created - create a zero-sized window and resize it once loaded
            Width = 0;
            Height = 0;
        }

        private void RestoreWindowPlacement()
        {
            WindowState = WindowState.Normal;

            if (IsWindowed)
            {
                WindowStyle = WindowStyle.SingleBorderWindow;

                var windowDimensions = ViewModel.GetWindowDimensions(
                    Constants.WindowedScreenHeightPropertyName,
                    DefaultWindowedHeight,
                    Constants.WindowedScreenWidthPropertyName,
                    DefaultWindowedWidth);

                Height = windowDimensions.Item1;
                Width = windowDimensions.Item2;
            }
            else
            {
                WindowState = WindowState.Normal;
            }

            _screenMapper.MapWindow(this);
        }
    }
}