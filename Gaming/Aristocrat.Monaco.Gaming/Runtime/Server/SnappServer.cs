namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Reflection;
    using Common;
    using Contracts;
    using Kernel;
    using log4net;
    using Snapp;

    public class SnappServer : IServerEndpoint, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()!?.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly SnappService _gameService;
        private readonly SnappReelService _reelService;
        private readonly SnappPresentationService _presentationService;
        private readonly object _lock = new ();

        private Server _server;
        private bool _disposed;

        public SnappServer(IEventBus eventBus, SnappService gameService, SnappReelService reelService, SnappPresentationService presentationService)
        {
            Logger.Debug("Create SnappServer");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
            _presentationService = presentationService ?? throw new ArgumentNullException(nameof(presentationService));

            Snapp.Logger.ExternalLogger = new SnappLogger(_eventBus);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            Logger.Debug("Start SnappServer");
            lock (_lock)
            {
                if (_server != null)
                {
                    Logger.Debug("(already started)");
                    return;
                }

                // Create service callback instances
                var callbacks = new ServiceCallbacks();
                callbacks.AddCallback(_gameService);
                callbacks.AddCallback(_reelService);
                callbacks.AddCallback(_presentationService);

                // Create and start server with transport (named pipe)
                _server = new Server(new NamedPipeTransport(GamingConstants.IpcPlatformPipeName), callbacks);
                _server.Start();
                Logger.Debug("(started)");
            }
        }

        public void Shutdown()
        {
            Logger.Debug("Shutdown SnappServer");
            lock (_lock)
            {
                if (_server == null)
                {
                    Logger.Debug("(no server to shut down)");
                    return;
                }

                try
                {
                    if (_server is IDisposable disposableServer)
                    {
                        Logger.Debug("(disposing server)");
                        disposableServer.Dispose();
                    }
                    Logger.Debug("(server disposed)");
                }
                catch (Exception ex) 
                {
                    Logger.Warn("Error while ending comms with", ex);
                }
                finally
                {
                    _server = null;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Shutdown();
            }

            _eventBus.UnsubscribeAll(this);

            _disposed = true;
        }
    }
}
