namespace Aristocrat.Monaco.G2S.Consumers
{
    using System;
    using System.Linq;
    using Accounting.Contracts;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Handlers;
    using Handlers.Voucher;
    using Meters;
    using Services;
    using VoucherExtensions = Handlers.Voucher.VoucherExtensions;

    /// <summary>
    ///     Handles the OperatorMenuEnteredEvent, which sets the cabinet's state.
    /// </summary>
    public class VoucherIssuedConsumer : Consumes<VoucherIssuedEvent>
    {
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly ITransactionReferenceProvider _references;
        private readonly ICommandBuilder<IVoucherDevice, voucherStatus> _statusCommandBuilder;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IMeterAggregator<IVoucherDevice> _voucherMeters;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherIssuedConsumer" /> class.
        /// </summary>
        public VoucherIssuedConsumer(
            IG2SEgm egm,
            ITransactionHistory transactionHistory,
            IEventLift eventLift,
            ICommandBuilder<IVoucherDevice, voucherStatus> statusCommandBuilder,
            ITransactionReferenceProvider references,
            IMeterAggregator<ICabinetDevice> cabinetMeters,
            IMeterAggregator<IVoucherDevice> voucherMeters)
        {
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));
            _statusCommandBuilder =
                statusCommandBuilder ?? throw new ArgumentNullException(nameof(statusCommandBuilder));
            _references = references ?? throw new ArgumentNullException(nameof(references));
            _cabinetMeters = cabinetMeters ?? throw new ArgumentNullException(nameof(cabinetMeters));
            _voucherMeters = voucherMeters ?? throw new ArgumentNullException(nameof(voucherMeters));
        }

        /// <inheritdoc />
        public override void Consume(VoucherIssuedEvent theEvent)
        {
            if (theEvent.Transaction == null)
            {
                return;
            }

            var device = _egm.GetDevice<IVoucherDevice>();
            if (device == null)
            {
                return;
            }

            var voucher = VoucherExtensions.GetIssueVoucher(theEvent.Transaction, _references);

            var log = theEvent.Transaction.ToLog(_references);

            var status = new voucherStatus();
            _statusCommandBuilder.Build(device, status);

            _eventLift.Report(
                device,
                EventCode.G2S_VCE103,
                device.DeviceList(status),
                voucher.transactionId,
                device.TransactionList(log),
                GetMeters(device));

            device.SendIssueVoucher(
                voucher,
                transactionId =>
                {
                    var trans = VoucherExtensions.FindTransaction<VoucherOutTransaction>(_transactionHistory, transactionId);

                    if (trans != null)
                    {
                        return trans.HostAcknowledged;
                    }

                    return false;
                },
                result =>
                {
                    var transaction = VoucherExtensions.FindTransaction<VoucherOutTransaction>(_transactionHistory, result);
                    if (transaction != null)
                    {
                        transaction.HostAcknowledged = true;
                        _transactionHistory.UpdateTransaction(transaction);

                        _eventLift.Report(
                            device,
                            EventCode.G2S_VCE105,
                            transaction.TransactionId,
                            device.TransactionList(transaction.ToLog(_references)));
                    }
                });
        }

        private meterList GetMeters(IVoucherDevice device)
        {
            var cabinetMeters = _cabinetMeters.GetMeters(
                _egm.GetDevice<ICabinetDevice>(),
                CabinetMeterName.PlayerCashableAmount,
                CabinetMeterName.PlayerPromoAmount,
                CabinetMeterName.PlayerNonCashableAmount);

            var voucherMeters = _voucherMeters.GetMeters(
                device,
                VoucherMeterName.CashableOutAmount,
                VoucherMeterName.CashableOutCount,
                VoucherMeterName.PromoOutAmount,
                VoucherMeterName.PromoOutCount,
                VoucherMeterName.NonCashableOutAmount,
                VoucherMeterName.NonCashableOutCount);

            return new meterList { meterInfo = cabinetMeters.Concat(voucherMeters).ToArray() };
        }
    }
}