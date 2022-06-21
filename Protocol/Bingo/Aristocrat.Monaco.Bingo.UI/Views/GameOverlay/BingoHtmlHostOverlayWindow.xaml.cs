namespace Aristocrat.Monaco.Bingo.UI.Views.GameOverlay
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using Models;
    using MVVM;
#if DEBUG
    using System.Windows.Input;
    using CefSharp;
#endif

    /// <summary>
    ///     Interaction logic for BingoHtmlHostOverlayViewModel.xaml
    /// </summary>
    public partial class BingoHtmlHostOverlayWindow
    {
        private readonly IBingoDisplayConfigurationProvider _bingoConfigurationProvider;
        private readonly BingoWindow _targetWindow;

#if DEBUG
        private bool _devToolsVisible;
#endif

        public BingoHtmlHostOverlayWindow(
            IBingoDisplayConfigurationProvider bingoConfigurationProvider,
            BingoWindow targetWindow,
            object viewModel)
        {
            _bingoConfigurationProvider = bingoConfigurationProvider ??
                                          throw new ArgumentNullException(nameof(bingoConfigurationProvider));
            _targetWindow = targetWindow;

            InitializeComponent();
            DataContext = viewModel;

            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;
#if DEBUG
            KeyDown += OnKeyDown;
#endif
        }

        protected override void OnClosing(CancelEventArgs e)
        {
#if DEBUG
            KeyDown -= OnKeyDown;
#endif
            BingoHtmlHost.Dispose();
            base.OnClosing(e);
        }

        private void BingoHtmlHostOverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = _bingoConfigurationProvider.GetWindow(_targetWindow);
            ConfigureDisplay(window);
        }

        private void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = e.NewSize.Width;
            Height = e.NewSize.Height;
        }

        private void ParentWindow_LocationChanged(object sender, EventArgs e)
        {
            Left = Owner.Left;
            Top = Owner.Top;
        }

#if DEBUG
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.F12)
            {
                return;
            }

            if (_devToolsVisible)
            {
                BingoHtmlHost.CloseDevTools();
                _devToolsVisible = false;
            }
            else
            {
                BingoHtmlHost.ShowDevTools();
                _devToolsVisible = true;
            }
        }
#endif

        private void ConfigureDisplay(Window window)
        {
            if (window == null)
            {
                return;
            }

            MvvmHelper.ExecuteOnUI(
                () =>
                {
                    if (Owner != null)
                    {
                        Owner.SizeChanged -= ParentWindow_SizeChanged;
                        Owner.LocationChanged -= ParentWindow_LocationChanged;
                        Owner = null;
                    }

                    Owner = window;
                    Owner.SizeChanged += ParentWindow_SizeChanged;
                    Owner.LocationChanged += ParentWindow_LocationChanged;

                    Top = window.Top;
                    Left = window.Left;

                    Width = window.Width;
                    Height = window.Height;

                    WindowState = window.WindowState;

                    BringIntoView();
                });
        }
    }
}
