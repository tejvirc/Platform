namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Reflection;
    using Common;
    using Contracts;
    using Grpc.Core;
    using log4net;
    using V1;

    public class RpcServer : IServerEndpoint, IDisposable
    {
        private const string Host = "localhost";

        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly RpcService _gameService;
        private readonly RpcReelService _reelService;
        private readonly RpcPresentationService _presentationService;
        private readonly object _lock = new object();

        private Server _rpcServer;

        private bool _disposed;

        public RpcServer(RpcService gameService, RpcReelService reelService, RpcPresentationService presentationService)
        {
            _gameService = gameService ?? throw new ArgumentNullException(nameof(gameService));
            _reelService = reelService ?? throw new ArgumentNullException(nameof(reelService));
            _presentationService = presentationService ?? throw new ArgumentNullException(nameof(presentationService));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            lock (_lock)
            {
                if (_rpcServer != null)
                {
                    return;
                }

                _rpcServer = new Server
                {
                    Services = { GameService.BindService(_gameService), ReelService.BindService(_reelService), PresentationService.BindService(_presentationService) },
                    Ports = { new ServerPort(Host, GamingConstants.IpcPort, ServerCredentials.Insecure) }
                };

                _rpcServer.Start();
            }
        }

        public void Shutdown()
        {
            lock (_lock)
            {
                if (_rpcServer == null)
                {
                    return;
                }

                try
                {
                    _rpcServer.KillAsync().FireAndForget();
                }
                catch (Exception ex)
                {
                    Logger.Warn("Error while ending comms with", ex);
                }
                finally
                {
                    _rpcServer = null;
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

            _disposed = true;
        }
    }
}