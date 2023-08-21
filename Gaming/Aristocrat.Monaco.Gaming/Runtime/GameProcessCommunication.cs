namespace Aristocrat.Monaco.Gaming.Runtime
{
    using System;
    using System.Collections.Generic;
    using Contracts.Process;
    using Server;

    /// <summary>
    ///     Service for communicating with a game process
    /// </summary>
    public class GameProcessCommunication : IProcessCommunication, IDisposable
    {
        private readonly IEnumerable<IServerEndpoint> _endpoints;
        private readonly object _lock = new object();

        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="GameProcessCommunication" /> class.
        /// </summary>
        public GameProcessCommunication(IEnumerable<IServerEndpoint> endpoints)
        {
            _endpoints = endpoints ?? throw new ArgumentNullException(nameof(endpoints));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void StartComms()
        {
            lock (_lock)
            {
                EndComms();
                foreach (var endpoint in _endpoints)
                {
                    endpoint.Start();
                }
            }
        }

        public void EndComms()
        {
            lock (_lock)
            {
                foreach (var endpoint in _endpoints)
                {
                    endpoint.Shutdown();
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
                EndComms();
            }

            _disposed = true;
        }
    }
}