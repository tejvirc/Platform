namespace Aristocrat.Monaco.Bingo.UI.Views.GameOverlay
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using CefSharp;
    using Models;
    using MVVM;
    using ViewModels.GameOverlay;
#if DEBUG
    using System.Windows.Input;
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
            object overlayViewModel)
        {
            _bingoConfigurationProvider = bingoConfigurationProvider ??
                                          throw new ArgumentNullException(nameof(bingoConfigurationProvider));
            _targetWindow = targetWindow;

            InitializeComponent();
            DataContext = overlayViewModel;

            BingoHelp.FrameLoadEnd += BingoHelp_OnFrameLoadEnd;
            BingoHelp.JavascriptMessageReceived += ViewModel.ExitHelp;

            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;
#if DEBUG
            KeyDown += OnKeyDown;
#endif
        }

        private BingoHtmlHostOverlayViewModel ViewModel => DataContext as BingoHtmlHostOverlayViewModel;

        private void BingoHtmlHostOverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = _bingoConfigurationProvider.GetWindow(_targetWindow);
            ConfigureDisplay(window);
        }

        public void BingoHelp_OnFrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            MvvmHelper.ExecuteOnUI(() => { ViewModel.IsHelpLoading = false; });

            if (e.Frame.IsMain)
            {
                BingoHelp.ExecuteScriptAsync(@"
                    document.addEventListener('click', function(e) {
                        CefSharp.PostMessage(e.target.id);
                    }, false);
                    window.addEventListener('onClose', function(e) {
                        CefSharp.PostMessage('Close');
                    }, false);
                ");
            }
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

                    BringIntoView();
                });
        }

        protected override void OnClosing(CancelEventArgs e)
        {
#if DEBUG
            KeyDown -= OnKeyDown;
#endif
            BingoHelp.JavascriptMessageReceived -= ViewModel.ExitHelp;
            BingoHelp.Dispose();

            BingoInfoHost.Dispose();
            base.OnClosing(e);
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
                BingoHelp.CloseDevTools();
                BingoInfoHost.CloseDevTools();
                _devToolsVisible = false;
            }
            else
            {
                if (ViewModel.IsHelpVisible)
                {
                    BingoHelp.ShowDevTools();
                }
                else
                {
                    BingoInfoHost.ShowDevTools();
                }

                _devToolsVisible = true;
            }
        }
#endif

        private void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Width = e.NewSize.Width;
            Height = e.NewSize.Height;
            ViewModel.LoadOverlay();
        }

        private void ParentWindow_LocationChanged(object sender, EventArgs e)
        {
            Left = Owner.Left;
            Top = Owner.Top;
        }
    }
}
