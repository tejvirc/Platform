namespace Aristocrat.Monaco.Mgam.Consumers
{
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Mgam.Client.Options;
    using Common;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Consumes the <see cref="HostConfigurationChangedEvent"/>.
    /// </summary>
    public class HostConfigurationChangedConsumer : Consumes<HostConfigurationChangedEvent>
    {
        private readonly IEventBus _eventBus;
        private readonly IOptionsMonitor<ProtocolOptions> _options;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HostConfigurationChangedConsumer"/> class.
        /// </summary>
        /// <param name="eventBus"><see cref="IEventBus"/>.</param>
        /// <param name="options"><see cref="IOptionsMonitor{TOptions}"/>.</param>
        public HostConfigurationChangedConsumer(
            IEventBus eventBus,
            IOptionsMonitor<ProtocolOptions> options)
        {
            _eventBus = eventBus;
            _options = options;
        }

        public override async Task Consume(HostConfigurationChangedEvent theEvent, CancellationToken cancellationToken)
        {
            IPAddress directoryAddress;

            if (!theEvent.UseUdpBroadcasting && !string.IsNullOrWhiteSpace(theEvent.DirectoryAddress))
            {
                directoryAddress = IPAddress.Parse(theEvent.DirectoryAddress);
            }
            else
            {
                directoryAddress = IPAddress.Broadcast;
            }

            _options.CurrentValue.DirectoryAddress = new IPEndPoint(directoryAddress, theEvent.DirectoryPort);

            _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.HostChanged));

            await Task.CompletedTask;
        }
    }
}
