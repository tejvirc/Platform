namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Handlers.NoteAcceptor;
    using Meters;

    /// <summary>
    ///     Handles the Note Acceptor <see cref="VoucherRedeemedEvent" />
    /// </summary>
    public class VoucherRedeemedConsumer : NoteAcceptorConsumerBase<VoucherRedeemedEvent>
    {
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;
        private readonly IG2SEgm _egm;
        private readonly IMeterAggregator<INoteAcceptorDevice> _meters;
        private readonly ITransactionHistory _transactionHistory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherRedeemedConsumer" /> class.
        /// </summary>
        /// <param name="egm">An <see cref="IG2SEgm" /> instance.</param>
        /// <param name="commandBuilder">An <see cref="ICommandBuilder{TDevice,TCommand}" /> implementation</param>
        /// <param name="meters">An <see cref="IMeterAggregator{TDevice}" /> instance</param>
        /// <param name="cabinetMeters">An <see cref="IMeterAggregator&lt;ICabinetDevice&gt;" /> instance</param>
        /// <param name="transactionHistory">An <see cref="ITransactionHistory" /> instance.</param>
        /// <param name="eventLift">A G2S event lift.</param>
        public VoucherRedeemedConsumer(
            IG2SEgm egm,
            ICommandBuilder<INoteAcceptorDevice, noteAcceptorStatus> commandBuilder,
            IMeterAggregator<INoteAcceptorDevice> meters,
            IMeterAggregator<ICabinetDevice> cabinetMeters,
            ITransactionHistory transactionHistory,
            IEventLift eventLift)
            : base(egm, commandBuilder, eventLift, EventCode.G2S_NAE115)
        {
            _egm = egm;
            _meters = meters ?? throw new ArgumentNullException(nameof(meters));
            _cabinetMeters = cabinetMeters ?? throw new ArgumentNullException(nameof(cabinetMeters));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
        }

        /// <inheritdoc />
        protected override bool ReportEvent(VoucherRedeemedEvent theEvent)
        {
            // The event is reported for success and failure. If the amount is zero it's a failure(?)
            return theEvent.Transaction.Amount > 0 && base.ReportEvent(theEvent);
        }

        /// <inheritdoc />
        protected override meterList GetMeters()
        {
            var cabinet = _egm.GetDevice<ICabinetDevice>();
            var noteAcceptor = _egm.GetDevice<INoteAcceptorDevice>();

            var cabinetMeters = _cabinetMeters.GetMeters(
                cabinet,
                CabinetMeterName.PlayerCashableAmount,
                CabinetMeterName.PlayerPromoAmount,
                CabinetMeterName.PlayerNonCashableAmount);

            var noteAcceptorMeters = _meters.GetMeters(
                noteAcceptor,
                CurrencyMeterName.CurrencyInAmount,
                CurrencyMeterName.CurrencyInCount,
                CurrencyMeterName.CurrencyToDropAmount,
                CurrencyMeterName.CurrencyToDropCount,
                CurrencyMeterName.CurrencyToDispenserAmount,
                CurrencyMeterName.NonCashableToDispAmount,
                CurrencyMeterName.PromoInAmount,
                CurrencyMeterName.NonCashableInAmount,
                CurrencyMeterName.PromoToDropAmount,
                CurrencyMeterName.NonCashableToDropAmount,
                CurrencyMeterName.PromoToDispAmount,
                CurrencyMeterName.NonCashableToDispAmount);

            return new meterList { meterInfo = cabinetMeters.Concat(noteAcceptorMeters).ToArray() };
        }

        /// <inheritdoc />
        protected override notesAcceptedLog GetLog()
        {
            var transaction =
                _transactionHistory.RecallTransactions<BillTransaction>()
                    .OrderByDescending(b => b.LogSequence)
                    .FirstOrDefault();

            return transaction == null ? base.GetLog() : transaction.ToNotesAcceptedLog();
        }
    }
}