namespace Aristocrat.Monaco.G2S.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Gaming.Contracts.Bonus;
    using Handlers;
    using Handlers.Bonus;
    using Kernel;
    using Meters;

    public class BonusService : IDisposable
    {
        private readonly IBonusHandler _bonusHandler;
        private readonly IMeterAggregator<IBonusDevice> _bonusMeters;
        private readonly IEventBus _bus;
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;
        private readonly ICommandBuilder<IBonusDevice, bonusStatus> _deviceStatus;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly IMeterAggregator<IHandpayDevice> _handpayMeters;
        private readonly IMeterAggregator<IVoucherDevice> _voucherMeters;

        private ActionBlock<BonusTransaction> _messageProcessor;
        private CancellationTokenSource _cancelProcessing;
        private volatile bool _canSend;

        private bool _disposed;

        public BonusService(
            IBonusHandler bonusHandler,
            IG2SEgm egm,
            ICommandBuilder<IBonusDevice, bonusStatus> deviceStatus,
            IMeterAggregator<ICabinetDevice> cabinetMeters,
            IMeterAggregator<IVoucherDevice> voucherMeters,
            IMeterAggregator<IHandpayDevice> handpayMeters,
            IMeterAggregator<IBonusDevice> bonusMeters,
            IEventBus bus,
            IEventLift eventLift)
        {
            _bonusHandler = bonusHandler ?? throw new ArgumentNullException(nameof(bonusHandler));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _deviceStatus = deviceStatus ?? throw new ArgumentNullException(nameof(deviceStatus));
            _cabinetMeters = cabinetMeters ?? throw new ArgumentNullException(nameof(cabinetMeters));
            _voucherMeters = voucherMeters ?? throw new ArgumentNullException(nameof(voucherMeters));
            _handpayMeters = handpayMeters ?? throw new ArgumentNullException(nameof(handpayMeters));
            _bonusMeters = bonusMeters ?? throw new ArgumentNullException(nameof(bonusMeters));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _eventLift = eventLift ?? throw new ArgumentNullException(nameof(eventLift));

            _bus.Subscribe<BonusPendingEvent>(this, HandleEvent);
            _bus.Subscribe<BonusAwardedEvent>(this, HandleEvent);
            _bus.Subscribe<BonusFailedEvent>(this, HandleEvent);
            _bus.Subscribe<BonusCancelledEvent>(this, HandleEvent);
            _bus.Subscribe<PartialBonusPaidEvent>(this, HandleEvent);
            _bus.Subscribe<DisplayLimitExceededEvent>(this, HandleEvent);
            _bus.Subscribe<CommunicationsStateChangedEvent>(this, evt => { _canSend = evt.Online; });

            _cancelProcessing = new CancellationTokenSource();

            _messageProcessor = new ActionBlock<BonusTransaction>(
                async request =>
                {
                    while (!_canSend && !_cancelProcessing.IsCancellationRequested)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1), _cancelProcessing.Token);
                    }

                    _cancelProcessing.Token.ThrowIfCancellationRequested();

                    while (!_cancelProcessing.IsCancellationRequested && !await HandleCommit(request))
                    {
                        var device = _egm.GetDevice<IBonusDevice>(request.DeviceId);

                        await Task.Delay(TimeSpan.FromMilliseconds(device.TimeToLive), _cancelProcessing.Token);
                    }
                });

            var committed = _bonusHandler.Transactions
                .Where(h => h.State == BonusState.Committed)
                .OrderBy(h => h.TransactionId).ToList();
            foreach (var bonus in committed)
            {
                _messageProcessor.Post(bonus);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _bus.UnsubscribeAll(this);

                _cancelProcessing.Cancel(false);
                _messageProcessor.Complete();
                _messageProcessor.Completion.Wait();

                _cancelProcessing.Dispose();
            }

            _messageProcessor = null;
            _cancelProcessing = null;

            _disposed = true;
        }

        private async Task<bool> HandleCommit(BonusTransaction bonus)
        {
            if (!ValidateProtocol(bonus.Protocol))
            {
                return true;
            }

            var device = _egm.GetDevice<IBonusDevice>(bonus.DeviceId);
            if (device == null)
            {
                return false;
            }

            var msg = new commitBonus
            {
                transactionId = bonus.TransactionId,
                bonusId = Convert.ToInt64(bonus.BonusId),
                bonusAwardAmt = bonus.TotalAmount,
                bonusMode = bonus.Mode.ToBonusMode(),
                creditType =
                    bonus.CashableAmount != 0 ? t_creditTypes.G2S_cashable :
                    bonus.NonCashAmount != 0 ? t_creditTypes.G2S_nonCash : t_creditTypes.G2S_promo,
                payMethod = bonus.PayMethod.ToBonusPayMethod(),
                expireCredits = false,
                //expireDateTime =
                idRestrict = !bonus.IdRequired ? t_idRestricts.G2S_none : t_idRestricts.G2S_anyId,
                //idReaderType = 
                playerId = bonus.PlayerId,
                bonusPaidAmt = bonus.PaidAmount,
                bonusDateTime = bonus.PaidDateTime,
                bonusException = bonus.Exception,
                amountWagered = bonus.MjtAmountWagered,
                bonusGamesPaid = bonus.MjtBonusGamesPaid,
                bonusGamesPlayed = bonus.MjtBonusGamesPlayed,
                idNumber = bonus.IdNumber
            };

            if (!await device.CommitBonus(msg))
            {
                return false;
            }

            _bonusHandler.Acknowledge(bonus.TransactionId);

            ReportEvent(EventCode.G2S_BNE107, bonus);

            return true;
        }

        private void HandleEvent(BonusPendingEvent evt)
        {
            if (!ValidateProtocol(evt.Transaction.Protocol))
            {
                return;
            }

            ReportEvent(EventCode.G2S_BNE103, evt.Transaction);
        }

        private void HandleEvent(BonusAwardedEvent evt)
        {
            if (!ValidateProtocol(evt.Transaction.Protocol))
            {
                return;
            }

            _messageProcessor.Post(evt.Transaction);

            ReportEvent(EventCode.G2S_BNE104, evt.Transaction, GetMeters(_egm.GetDevice<IBonusDevice>(evt.Transaction.DeviceId)));
        }

        private void HandleEvent(BonusFailedEvent evt)
        {
            if (!ValidateProtocol(evt.Transaction.Protocol))
            {
                return;
            }

            _messageProcessor.Post(evt.Transaction);

            ReportEvent(EventCode.G2S_BNE105, evt.Transaction);

            if (evt.Transaction.Mode == BonusMode.WagerMatch &&
                (BonusException)evt.Transaction.Exception == BonusException.PayMethodNotAvailable)
            {
                ReportEvent(EventCode.G2S_BNE201, evt.Transaction);
            }
        }

        private void HandleEvent(BonusCancelledEvent evt)
        {
            if (!ValidateProtocol(evt.Transaction.Protocol))
            {
                return;
            }

            _messageProcessor.Post(evt.Transaction);

            ReportEvent(EventCode.G2S_BNE106, evt.Transaction);
        }

        private void HandleEvent(PartialBonusPaidEvent evt)
        {
            if (!ValidateProtocol(evt.Transaction.Protocol))
            {
                return;
            }

            ReportEvent(EventCode.IGT_BNE005, evt.Transaction, GetMeters(_egm.GetDevice<IBonusDevice>(evt.Transaction.DeviceId)));
        }

        private void HandleEvent(DisplayLimitExceededEvent evt)
        {
            if (!ValidateProtocol(evt.Transaction.Protocol))
            {
                return;
            }

            _eventLift.Report(_egm.GetDevice<IBonusDevice>(evt.Transaction.DeviceId), EventCode.IGT_BNE003);

            if (evt.Transaction.Mode == BonusMode.WagerMatch)
            {
                // Since both value are evaluated we're going to generate both events
                _eventLift.Report(_egm.GetDevice<IBonusDevice>(evt.Transaction.DeviceId), EventCode.IGT_BNE004);
            }
        }

        private bool ValidateProtocol(CommsProtocol protocol)
        {
            return protocol == CommsProtocol.None || protocol == CommsProtocol.G2S;
        }

        private void ReportEvent(string eventCode, BonusTransaction transaction, meterList meters = null)
        {
            if (!ValidateProtocol(transaction.Protocol))
            {
                return;
            }

            var device = _egm.GetDevice<IBonusDevice>(transaction.DeviceId);

            var status = new bonusStatus();

            _deviceStatus.Build(device, status);

            _eventLift.Report(
                device,
                eventCode,
                device.DeviceList(status),
                transaction.TransactionId,
                device.TransactionList(transaction.ToBonusLog()),
                meters);
        }

        private meterList GetMeters(IBonusDevice device)
        {
            var cabinetMeters = new List<meterInfo>(
                _cabinetMeters.GetMeters(
                    _egm.GetDevice<ICabinetDevice>(),
                    CabinetMeterName.PlayerCashableAmount,
                    CabinetMeterName.PlayerPromoAmount,
                    CabinetMeterName.PlayerNonCashableAmount,
                    CabinetMeterName.CardedBonusWonAmount,

                    CabinetMeterName.EgmPaidBonusWonAmount,
                    CabinetMeterName.EgmPaidBonusNonWonAmount,

                    CabinetMeterName.MjtGamesPlayedCount,
                    CabinetMeterName.MjtGamesPaidCount,
                    CabinetMeterName.MjtBonusAmount,
                    CabinetMeterName.WagerMatchPlayedCount,
                    CabinetMeterName.WagerMatchBonusAmount,

                    CabinetMeterName.HandPaidBonusWonAmount,
                    CabinetMeterName.HandPaidBonusNonWonAmount,
                    CabinetMeterName.HandPaidCancelAmount));

            var voucherMeters = _voucherMeters.GetMeters(
                _egm.GetDevice<IVoucherDevice>(),
                VoucherMeterName.CashableOutAmount,
                VoucherMeterName.PromoOutAmount,
                VoucherMeterName.NonCashableOutAmount,
                VoucherMeterName.CashableOutCount);

            var handpayMeters = _handpayMeters.GetMeters(
                _egm.GetDevice<IHandpayDevice>(),
                TransferMeterName.CashableOutAmount,
                TransferMeterName.PromoOutAmount,
                TransferMeterName.NonCashableOutAmount,
                TransferMeterName.TransferOutCount);

            var bonusMeters = _bonusMeters.GetMeters(
                device,
                TransferMeterName.CashableInAmount,
                TransferMeterName.PromoInAmount,
                TransferMeterName.NonCashableInAmount,
                TransferMeterName.TransferInCount);

            return new meterList
            {
                meterInfo = cabinetMeters
                    .Concat(bonusMeters)
                    .Concat(voucherMeters)
                    .Concat(handpayMeters)
                    .ToArray()
            };
        }
    }
}