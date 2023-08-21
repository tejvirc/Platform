namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Common.Events;
    using Kernel;

    /// <summary>
    ///     Handles the <see cref="CertificateRenewedEvent" /> event.
    /// </summary>
    public class CertificateRenewedConsumer : Consumes<CertificateRenewedEvent>
    {
        private readonly IEventBus _bus;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateRenewedConsumer" /> class.
        /// </summary>
        /// <param name="bus">An <see cref="IEventBus" /> instance</param>
        public CertificateRenewedConsumer(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public override void Consume(CertificateRenewedEvent theEvent)
        {
            _bus.Publish(new RestartProtocolEvent());
        }
    }
}