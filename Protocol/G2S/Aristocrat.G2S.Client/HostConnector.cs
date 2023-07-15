namespace Aristocrat.G2S.Client
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Communications;

    /// <summary>
    ///     Defines a new instance of an IHostConnector.
    /// </summary>
    public class HostConnector : IHostConnector
    {
        private readonly ICommandDispatcher _dispatcher;
        private readonly IEgm _egm;
        private readonly ISendEndpointProvider _endpointProvider;
        private readonly ConcurrentDictionary<int, IHostControl> _hosts = new ConcurrentDictionary<int, IHostControl>();
        private readonly IIdProvider<int> _idProvider;
        private readonly IMessageReceiverConnector _messageReceiverConnector;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConnector" /> class.
        /// </summary>
        /// <param name="egm">The egm</param>
        /// <param name="endpointProvider">An instance of an ISendEndpointProvider.</param>
        /// <param name="dispatcher">The command dispatcher</param>
        /// <param name="idProvider">The id provider</param>
        /// <param name="messageReceiverConnector">An instance of a IReceiveEndpoint</param>
        public HostConnector(
            IEgm egm,
            ISendEndpointProvider endpointProvider,
            ICommandDispatcher dispatcher,
            IdProvider<int> idProvider,
            IMessageReceiverConnector messageReceiverConnector)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _endpointProvider = endpointProvider ?? throw new ArgumentNullException(nameof(endpointProvider));
            _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));
            _messageReceiverConnector =
                messageReceiverConnector ?? throw new ArgumentNullException(nameof(messageReceiverConnector));
        }

        /// <inheritdoc />
        public IEnumerable<IHostControl> Hosts => _hosts.Values;

        /// <inheritdoc />
        public IHostControl GetHostById(int hostId)
        {
            _hosts.TryGetValue(hostId, out var host);

            return host;
        }

        /// <inheritdoc />
        public IHostControl RegisterHost(int hostId, Uri hostUri, bool requiredForPlay, int index)
        {
            return _hosts.GetOrAdd(
                hostId,
                id =>
                {
                    var queue = new HostQueue(hostId, _egm, _endpointProvider, _dispatcher, _idProvider);

                    var host = new RegisteredHost(hostId, hostUri, requiredForPlay, index, _endpointProvider, queue);

                    _messageReceiverConnector.Connect(queue);

                    return host;
                });
        }

        /// <inheritdoc />
        public IHost UnregisterHost(int hostId, IEgmStateManager egmStateManager = null)
        {
            if (_hosts.TryRemove(hostId, out var host))
            {
                if (host != null)
                {
                    host.Stop();

                    _messageReceiverConnector.Disconnect(host.Queue);
                }
            }

            return host;
        }
    }
}