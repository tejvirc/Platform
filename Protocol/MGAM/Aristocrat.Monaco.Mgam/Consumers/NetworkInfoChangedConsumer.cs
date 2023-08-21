namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Mgam.Client.Options;
    using Common;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Consumes the <see cref="NetworkInfoChangedEvent"/>.
    /// </summary>
    public class NetworkInfoChangedConsumer : Consumes<NetworkInfoChangedEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IOptionsMonitor<ProtocolOptions> _options;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NetworkInfoChangedConsumer"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/>.</param>
        public NetworkInfoChangedConsumer(
            IEventBus eventBus,
            IOptionsMonitor<ProtocolOptions> options)
        {
            _eventBus = eventBus;
            _options = options;
        }

        /// <inheritdoc />
        public override Task Consume(NetworkInfoChangedEvent theEvent, CancellationToken cancellationToken)
        {
            _options.CurrentValue.NetworkAddress = NetworkInterfaceInfo.DefaultIpAddress;

            _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.NetworkChanged));

            return Task.CompletedTask;
        }
    }
}
