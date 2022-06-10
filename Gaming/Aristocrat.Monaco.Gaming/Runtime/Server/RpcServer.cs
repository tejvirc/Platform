namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Reflection;
    using Contracts;
    using Kernel;
    using log4net;
    using Snapp;

    public class RpcServer : IServerEndpoint, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        private readonly IEventBus _eventBus;
        private readonly RpcService _gameService;
        private readonly RpcReelService _reelService;
        private readonly RpcPresentationService _presentationService;
        private readonly object _lock = new ();

        private ITransport _transport;
        private Server _server;
        private bool _disposed;

        public RpcServer(IEventBus eventBus, RpcService gameService, RpcReelService reelService, RpcPresentationService presentationService)
        {
            Logger.Debug("Create RpcServer");
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
            _presentationService = presentationService ?? throw new ArgumentNullException(nameof(presentationService));

            Snapp.Logger.ExternalLogger = new RpcLogger(_eventBus);

            _eventBus.Subscribe<GameProcessExitedEvent>(this, _ => Shutdown());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            Logger.Debug("Start RpcServer");
            lock (_lock)
            {
                if (_server != null)
                {
                    return;
                }

                // Create service callback instances
                var callbacks = new ServiceCallbacks();
                callbacks.AddCallback(_gameService);
                callbacks.AddCallback(_reelService);
                callbacks.AddCallback(_presentationService);

                // Create and start server with transport (named pipe)
                _transport = new NamedPipeTransport(GamingConstants.IpcPipeName);
                _server = new Server(_transport, callbacks);
                _server.Start();
            }
        }

        public void Shutdown()
        {
            lock (_lock)
            {
                if (_server == null)
                {
                    return;
                }

                try
                {
                    _transport?.Disconnect();
                }
                catch (Exception ex)
                {
                    Logger.Warn("Error while disconnecting transport; probably it wasn't connected (yet)", ex);
                }
                finally
                {
                    try
                    {
                        _transport?.Dispose();
                        _server?.Stop();
                    }
                    catch (Exception ex)
                    {
                        Logger.Warn("Error while ending comms with", ex);
                    }
                    finally
                    {
                        _transport = null;
                        _server = null;
                    }
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

                lock (_lock)
                {
                    if (_server != null)
                    {
                        _server.Dispose();
                        _server = null;
                    }

                    if (_transport != null)
                    {
                        _transport.Dispose();
                        _transport = null;
                    }
                }
            }

            _eventBus.UnsubscribeAll(this);

            _disposed = true;
        }
    }
}