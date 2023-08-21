namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Common.Events;

    /// <summary>
    ///     Handles the <see cref="CertificateValidatedEvent" /> event.
    /// </summary>
    public class CertificateValidatedConsumer : Consumes<CertificateValidatedEvent>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateValidatedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        public CertificateValidatedConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(CertificateValidatedEvent theEvent)
        {
            if (_egm != null && !_egm.Running)
            {
                var context = new StartupContext();
                _egm.Start(context.ContextPerHost(_egm.Hosts));
            }
        }
    }
}