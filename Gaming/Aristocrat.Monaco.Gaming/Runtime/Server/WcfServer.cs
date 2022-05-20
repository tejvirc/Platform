namespace Aristocrat.Monaco.Gaming.Runtime.Server
{
    using System;
    using System.Reflection;
    using System.ServiceModel;
    using GDKRuntime.Client;
    using GDKRuntime.Contract;
    using log4net;

    public class WcfServer : IServerEndpoint, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object _lock = new object();
        private readonly IGameSession _gameSession;

        private ServiceHost _serviceHost;

        private bool _disposed;

        public WcfServer(IGameSession gameSession)
        {
            _gameSession = gameSession;
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
                if (_serviceHost?.State == CommunicationState.Opened ||
                    _serviceHost?.State == CommunicationState.Created)
                {
                    return;
                }

                Shutdown();
                _serviceHost = new ServiceHost(_gameSession);
                _serviceHost.AddServiceEndpoint(GDKRuntimeClient.EndPoint());
                _serviceHost.Open();
            }
        }

        public void Shutdown()
        {
            lock (_lock)
            {
                if (_serviceHost == null)
                {
                    return;
                }

                try
                {
                    _serviceHost.Close();
                }
                catch (Exception ex)
                {
                    Logger.Warn("Error while ending comms with", ex);
                    _serviceHost.Abort();
                }
                finally
                {
                    _serviceHost = null;
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