namespace Aristocrat.Monaco.Bingo.UI.Views.GameOverlay
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using CefSharp;
    using Models;
    using MVVM;
    using ViewModels.GameOverlay;

    public partial class BingoHelpOverlayWindow
    {
        private readonly IBingoDisplayConfigurationProvider _bingoConfigurationProvider;
        private readonly BingoWindow _targetWindow;

        public BingoHelpOverlayWindow(
            IBingoDisplayConfigurationProvider bingoConfigurationProvider,
            BingoWindow targetWindow,
            object viewModel)
        {
            _bingoConfigurationProvider = bingoConfigurationProvider ??
                                          throw new ArgumentNullException(nameof(bingoConfigurationProvider));
            _targetWindow = targetWindow;

            InitializeComponent();
            DataContext = viewModel;
            BingoHelp.FrameLoadEnd += BingoHelp_OnFrameLoadEnd;

            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;
        }

        private BingoHelpOverlayViewModel ViewModel => DataContext as BingoHelpOverlayViewModel;

        public void BingoHelp_OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            MvvmHelper.ExecuteOnUI(() => { ViewModel.IsLoading = false; });

            if (e.Frame.IsMain)
            {
                BingoHelp.ExecuteScriptAsync(@"
                    document.addEventListener('click', function(e) {
                        CefSharp.PostMessage(e.target.id);
                    }, false);
                ");
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            BingoHelp?.Dispose();
            base.OnClosing(e);
        }

        private void BingoHelpOverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            MvvmHelper.ExecuteOnUI(() => { ViewModel.IsLoading = true; });

            var window = _bingoConfigurationProvider.GetWindow(_targetWindow);
            ConfigureDisplay(window);
        }

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

                    ViewModel.LoadBingoHelp();

                    BringIntoView();
                });
        }

        private void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = Owner.Width;
            Height = Owner.Height;

            ViewModel.LoadBingoHelp();
        }

        private void ParentWindow_LocationChanged(object sender, EventArgs e)
        {
            Left = Owner.Left;
            Top = Owner.Top;
        }
    }
}
