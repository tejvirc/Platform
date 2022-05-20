namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using Common.Events;
    using Services;

    /// <summary>
    ///     Handles the <see cref="CommunicationsStateChangedEvent" />.
    /// </summary>
    public class CommunicationsStateChangedConsumer : Consumes<CommunicationsStateChangedEvent>
    {
        private readonly IVoucherDataService _voucherDataService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommunicationsStateChangedConsumer" /> class.
        /// </summary>
        /// <param name="voucherDataService">Voucher data service.</param>
        public CommunicationsStateChangedConsumer(
            IVoucherDataService voucherDataService)
        {
            _voucherDataService = voucherDataService ?? throw new ArgumentNullException(nameof(voucherDataService));
        }

        /// <inheritdoc />
        public override void Consume(CommunicationsStateChangedEvent theEvent)
        {
            _voucherDataService.CommunicationsStateChanged(theEvent.HostId, theEvent.Online);
        }
    }
}