namespace Aristocrat.Monaco.Bingo.UI.ViewModels.GameOverlay
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using CefSharp;
    using Common;
    using Common.Events;
    using Events;
    using Gaming.Contracts;
    using Gaming.Contracts.Events;
    using Kernel;
    using MVVM.Model;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Viewmodel for bingo dynamic help overlay window.
    /// </summary>
    public class BingoHelpOverlayViewModel : BaseNotify, IDisposable
    {
        private const double MinHelpBoxMarginLeft = 0.1;
        private const double MinHelpBoxMarginTop = 0.1;
        private const double MinHelpBoxMarginRight = 0.1;
        private const double MinHelpBoxMarginBottom = 0.24;
        private const string CloseEvent = "Close";

        private readonly SolidColorBrush _blueBrush = new(Colors.Blue);
        private readonly SolidColorBrush _transparentBrush = new(Colors.Transparent);

        private readonly IDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IBingoDisplayConfigurationProvider _bingoConfigurationProvider;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private BingoDisplayConfigurationHelpAppearance _helpAppearance;
        private Thickness _helpBoxMargin;
        private string _address;
        private bool _disposed;
        private bool _isLoading;
        private bool _visible;
        private SolidColorBrush _colorBrush;
        private double _height;
        private double _width;
        private IWebBrowser _webBrowser;

        public BingoHelpOverlayViewModel(
            IDispatcher dispatcher,
            IEventBus eventBus,
            IPropertiesManager propertiesManager,
            IBingoDisplayConfigurationProvider bingoConfigurationProvider,
            IUnitOfWorkFactory unitOfWorkFactory)
        {
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _bingoConfigurationProvider = bingoConfigurationProvider ?? throw new ArgumentNullException(nameof(bingoConfigurationProvider));
            _unitOfWorkFactory = unitOfWorkFactory;
            _colorBrush = _transparentBrush;

            _eventBus.Subscribe<HostConnectedEvent>(this, HandleEvent);
            _eventBus.Subscribe<GameLoadedEvent>(this, _ => SetVisibility(false));
            _eventBus.Subscribe<GameExitedNormalEvent>(this, _ => SetVisibility(false));
            _eventBus.Subscribe<GameFatalErrorEvent>(this, _ => SetVisibility(false));
            _eventBus.Subscribe<BingoDisplayHelpAppearanceChangedEvent>(this, evt => UpdateAppearance(evt.HelpAppearance));
            _eventBus.Subscribe<GameRequestedPlatformHelpEvent>(this, HandleHelpRequested);
            _eventBus.Subscribe<BingoHelpTestToolTabVisibilityChanged>(this, evt => SetBorderVisibility(evt.Visible));
        }

        public void LoadBingoHelp()
        {
            UpdateAppearance(_bingoConfigurationProvider.GetHelpAppearance());
        }

        public Thickness HelpBoxMargin
        {
            get => _helpBoxMargin;
            private set => SetProperty(ref _helpBoxMargin, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public bool Visible
        {
            get => _visible;
            set => SetProperty(ref _visible, value);
        }

        public SolidColorBrush ColorBrush
        {
            get => _colorBrush;
            set => SetProperty(ref _colorBrush, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public IWebBrowser WebBrowser
        {
            get => _webBrowser;
            set => SetProperty(ref _webBrowser, value);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void HandleHelpRequested(GameRequestedPlatformHelpEvent evt)
        {
            _dispatcher.ExecuteOnUIThread(() => SetVisibility(evt.Visible));
        }

        private void UpdateAppearance(BingoDisplayConfigurationHelpAppearance appearance)
        {
            _dispatcher.ExecuteOnUIThread(
                () =>
                {
                    _helpAppearance = appearance;

                    HelpBoxMargin = new Thickness(
                        (_helpAppearance.HelpBox.Left > MinHelpBoxMarginLeft ? MinHelpBoxMarginLeft : _helpAppearance.HelpBox.Left) * Width,
                        (_helpAppearance.HelpBox.Top > MinHelpBoxMarginTop ? MinHelpBoxMarginTop : _helpAppearance.HelpBox.Top) * Height,
                        (_helpAppearance.HelpBox.Right > MinHelpBoxMarginRight ? MinHelpBoxMarginRight : _helpAppearance.HelpBox.Right) * Width,
                        (_helpAppearance.HelpBox.Bottom > MinHelpBoxMarginBottom ? MinHelpBoxMarginBottom : _helpAppearance.HelpBox.Bottom) * Height);

                    Address = _unitOfWorkFactory.GetHelpUri(_propertiesManager).ToString();
                });
        }

        private void HandleEvent(HostConnectedEvent evt)
        {
            _dispatcher.ExecuteOnUIThread(() => Address = _unitOfWorkFactory.GetHelpUri(_propertiesManager).ToString());
        }

        private void SetVisibility(bool visible)
        {
            _dispatcher.ExecuteOnUIThread(
                () =>
                {
                    if (visible)
                    {
                        Address = _unitOfWorkFactory.GetHelpUri(_propertiesManager).ToString();
                    }

                    Visible = visible;
                });
        }

        private void SetBorderVisibility(bool visible)
        {
            _dispatcher.ExecuteOnUIThread(() => ColorBrush = visible ? _blueBrush : _transparentBrush);
        }

        public void ExitHelp(object sender, JavascriptMessageReceivedEventArgs args)
        {
            if (args.Message is not CloseEvent)
            {
                return;
            }

            _eventBus.Publish(new ExitHelpEvent());
        }
    }
}
