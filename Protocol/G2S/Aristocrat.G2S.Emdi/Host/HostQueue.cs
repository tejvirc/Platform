namespace Aristocrat.G2S.Emdi.Host
{
    using Handlers;
    using Serialization;
    using SimpleInjector;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Monaco.Application.Contracts.Media;

    /// <summary>
    ///     Implements <see cref="IHostQueue"/> interface
    /// </summary>
    public class HostQueue : IHostQueue, IDisposable
    {
        private readonly Container _container;
        private readonly Dictionary<int, IHost> _hosts;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostQueue"/> class.
        /// </summary>
        /// <param name="container"></param>
        public HostQueue(Container container)
        {
            _container = container;

            _hosts = new Dictionary<int, IHost>();
        }

        /// <inheritdoc />
        ~HostQueue()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public async Task StartAsync(int port)
        {
            if (!_hosts.ContainsKey(port))
            {
                var host = new HostService(
                    _container.GetInstance<IMediaProvider>(),
                    _container.GetInstance<IMessageSerializer>(),
                    _container.GetInstance<ICommandHandlerFactory>());

                _hosts.Add(port, host);
            }

            await _hosts[port].StartAsync(new HostConfiguration(port));
        }

        /// <inheritdoc />
        public async Task StopAsync(int port)
        {
            if (!_hosts.ContainsKey(port))
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            var host = _hosts[port];

            await host.StopAsync();

            host.Dispose();

            _hosts.Remove(port);
        }

        /// <inheritdoc />
        public async Task StopAllAsync()
        {
            foreach (var port in _hosts.Keys.ToArray())
            {
                await StopAsync(port);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();

            if (disposing)
            {
                foreach (var host in _hosts.Values)
                {
                    host.Dispose();
                }
            }
        }

        private void ReleaseUnmanagedResources()
        {
        }

        /// <inheritdoc />
        public IHost this[int port] => _hosts[port];

        /// <inheritdoc />
        public IEnumerator<IHost> GetEnumerator()
        {
            return _hosts.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
