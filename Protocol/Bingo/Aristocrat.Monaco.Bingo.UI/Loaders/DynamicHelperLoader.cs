namespace Aristocrat.Monaco.Bingo.UI.Loaders
{
    using System;
    using Common;
    using Kernel;
    using Models;
    using Protocol.Common.Storage.Entity;
    using ViewModels.GameOverlay;
    using Views.GameOverlay;

    public class DynamicHelperLoader : IBingoPresentationLoader, IDisposable
    {
        private readonly IDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _propertiesManager;
        private readonly IBingoDisplayConfigurationProvider _bingoConfigurationProvider;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private BingoHelpOverlayViewModel _viewModel;
        private BingoHelpOverlayWindow _overlayWindow;
        private bool _disposed;

        public DynamicHelperLoader(
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
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
        }

        public void LoadPresentation()
        {
            _dispatcher.ExecuteOnUIThread(LoadUI);
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
                ClearOverlayWindow();
            }

            _disposed = true;
        }

        private void ClearOverlayWindow()
        {
            if (_overlayWindow is not null)
            {
                _overlayWindow.Close();
                _overlayWindow.Dispose();
            }

            _overlayWindow = null;

            if (_viewModel is not null)
            {
                _viewModel.Dispose();
            }

            _viewModel = null;
        }

        private void LoadUI()
        {
            ClearOverlayWindow();
            _viewModel = new BingoHelpOverlayViewModel(
                _dispatcher,
                _eventBus,
                _propertiesManager,
                _bingoConfigurationProvider,
                _unitOfWorkFactory);
            _overlayWindow = new BingoHelpOverlayWindow(
                _bingoConfigurationProvider,
                BingoWindow.Main,
                _viewModel);

            _overlayWindow.Show();
        }
    }
}