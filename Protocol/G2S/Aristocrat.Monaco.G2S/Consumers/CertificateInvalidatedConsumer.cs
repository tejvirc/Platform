namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Aristocrat.G2S.Client;
    using Common.Events;

    /// <summary>
    ///     Handles the <see cref="CertificateInvalidatedEvent" /> event.
    /// </summary>
    public class CertificateInvalidatedConsumer : Consumes<CertificateInvalidatedEvent>
    {
        private readonly IG2SEgm _egm;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CertificateInvalidatedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        public CertificateInvalidatedConsumer(IG2SEgm egm)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
        }

        /// <inheritdoc />
        public override void Consume(CertificateInvalidatedEvent theEvent)
        {
            _egm?.Stop();
        }
    }
}