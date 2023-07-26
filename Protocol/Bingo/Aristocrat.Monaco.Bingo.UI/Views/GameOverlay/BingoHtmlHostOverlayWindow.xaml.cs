namespace Aristocrat.Monaco.Bingo.UI.Views.GameOverlay
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using CefSharp;
    using Models;
    using Monaco.UI.Common.CefHandlers;
    using ViewModels.GameOverlay;
    using Aristocrat.Toolkit.Mvvm.Extensions;
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
            BingoHelpHost.ExecuteScriptAsyncWhenPageLoaded(
                @"
                    window.addEventListener('onClose', function(e) {
                        CefSharp.PostMessage('Close');
                    }, false);
                    window.addEventListener('click', function(e) {
                        CefSharp.PostMessage('Click');
                    }, false);
                ",
                false);

            BingoHelpHost.JavascriptMessageReceived += ViewModel.HandleJavascriptMessageReceived;

            SetupBrowsers(BingoInfoHost);
            SetupBrowsers(BingoHelpHost);
            SetupBrowsers(DynamicMessageHost);

            // MetroApps issue--need to set in code behind after InitializeComponent.
            AllowsTransparency = true;
#if DEBUG
            KeyDown += OnKeyDown;
#endif
        }

        private static void SetupBrowsers(IWebBrowser browser)
        {
            browser.MenuHandler = new DisabledContextMenuHandler();
            browser.JsDialogHandler = new JsDialogHandler();
            browser.DownloadHandler = new DownloadHandler();
            browser.DragHandler = new DragHandler();
            browser.DialogHandler = new DialogHandler();
            browser.DisplayHandler = new DisplayHandler();
        }

        private BingoHtmlHostOverlayViewModel ViewModel => DataContext as BingoHtmlHostOverlayViewModel;

        private void BingoHtmlHostOverlayWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var window = _bingoConfigurationProvider.GetWindow(_targetWindow);
            ConfigureDisplay(window);
        }

        private void ConfigureDisplay(Window window)
        {
            if (window == null)
            {
                return;
            }

            Execute.OnUIThread(
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
            BingoHelpHost.JavascriptMessageReceived -= ViewModel.HandleJavascriptMessageReceived;
            BingoHelpHost.Dispose();
            BingoInfoHost.Dispose();
            DynamicMessageHost.Dispose();
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
                BingoHelpHost.CloseDevTools();
                BingoInfoHost.CloseDevTools();
                DynamicMessageHost.CloseDevTools();
                _devToolsVisible = false;
            }
            else
            {
                if (ViewModel.IsHelpVisible)
                {
                    BingoHelpHost.ShowDevTools();
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
