﻿namespace Aristocrat.Monaco.Bingo.UI.Loaders
{
    using System;
    using Common;
    using Gaming.Contracts;
    using Kernel;
    using Models;
    using OverlayServer;
    using Services;
    using ViewModels.GameOverlay;
    using Views.GameOverlay;

    public class BingoOverlayLoader : IBingoPresentationLoader, IDisposable
    {
        private readonly IPropertiesManager _propertiesManager;
        private readonly IDispatcher _dispatcher;
        private readonly IEventBus _eventBus;
        private readonly IBingoDisplayConfigurationProvider _bingoConfigurationProvider;
        private readonly ILegacyAttractProvider _attractProvider;
        private readonly IGameProvider _gameProvider;
        private readonly IServer _server;

        private BingoHtmlHostOverlayViewModel _viewModel;
        private BingoHtmlHostOverlayWindow _overlayWindow;
        private bool _disposed;

        public BingoOverlayLoader(
            IPropertiesManager propertiesManager,
            IDispatcher dispatcher,
            IEventBus eventBus,
            IBingoDisplayConfigurationProvider bingoConfigurationProvider,
            ILegacyAttractProvider attractProvider,
            IGameProvider gameProvider,
            IServer server)
        {
            _propertiesManager = propertiesManager ?? throw new ArgumentNullException(nameof(propertiesManager));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _bingoConfigurationProvider = bingoConfigurationProvider ?? throw new ArgumentNullException(nameof(bingoConfigurationProvider));
            _attractProvider = attractProvider ?? throw new ArgumentNullException(nameof(attractProvider));
            _gameProvider = gameProvider ?? throw new ArgumentNullException(nameof(gameProvider));
            _server = server ?? throw new ArgumentNullException(nameof(server));
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
            _viewModel = new BingoHtmlHostOverlayViewModel(
                _propertiesManager,
                _dispatcher,
                _eventBus,
                _bingoConfigurationProvider,
                _attractProvider,
                _gameProvider,
                _server);
            _overlayWindow = new BingoHtmlHostOverlayWindow(_bingoConfigurationProvider, BingoWindow.Main, _viewModel);
            _overlayWindow.Show();
        }
    }
}