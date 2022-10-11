namespace Aristocrat.Monaco.G2S.Services
{
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Handlers;
    using Handlers.Handpay;
    using Kernel;
    using log4net;
    using Meters;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Accounting.Contracts.Transactions;
    using Application.Contracts;
    using Handlers.Voucher;
    using Application.Contracts.Localization;
    using Localization.Properties;

    /// <summary>
    /// Implements <see cref="IHandpayService"/> interface
    /// </summary>
    public class HandpayService : IHandpayService, IHandpayValidator, IHandpayProperties, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private const int RetryTimeout = 300;

        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IG2SEgm _egm;
        private readonly IEventLift _eventLift;
        private readonly ITransactionHistory _transactionHistory;
        private readonly ITransactionReferenceProvider _references;
        private readonly ICommandBuilder<IHandpayDevice, handpayStatus> _statusCommandBuilder;

        private readonly IMeterAggregator<IHandpayDevice> _handpayMeters;
        private readonly IMeterAggregator<ICabinetDevice> _cabinetMeters;

        private readonly ActionBlock<long> _retryCommandsQueue;

        private bool _disposed;

        public HandpayService(
            IEventBus eventBus,
            IPropertiesManager properties,
            IG2SEgm egm,
            IEventLift eventLift,
            ITransactionHistory transactionHistory,
            ITransactionReferenceProvider references,
            ICommandBuilder<IHandpayDevice, handpayStatus> statusCommandBuilder,
            IMeterAggregator<IHandpayDevice> handpayMeters,
            IMeterAggregator<ICabinetDevice> cabinetMeters)
        {
            _eventBus = eventBus;
            _properties = properties;
            _egm = egm;
            _eventLift = eventLift;
            _transactionHistory = transactionHistory;
            _references = references;
            _statusCommandBuilder = statusCommandBuilder;
            _handpayMeters = handpayMeters;
            _cabinetMeters = cabinetMeters;

            _retryCommandsQueue = new ActionBlock<long>(
                async transactionId =>
                {
                    try
                    {
                        await RetryCommand(transactionId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error re-sending handpay command to the host for Transaction {transactionId}.", ex);
                        await _retryCommandsQueue.SendAsync(transactionId);
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 1,
                    EnsureOrdered = true,
                    BoundedCapacity = DataflowBlockOptions.Unbounded,
                    MaxMessagesPerTask = DataflowBlockOptions.Unbounded,
                });

            _retryCommandsQueue.Completion.ContinueWith(
                t =>
                {
                    if (!t.IsFaulted)
                    {
                        return;
                    }

                    var ex = t.Exception == null ? new InvalidOperationException() : (Exception)t.Exception.Flatten();
                    Logger.Error("Error retrying to send handpay commands", ex);
                });
        }

        /// <inheritdoc />
        ~HandpayService()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public string Name => GetType().ToString();

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHandpayService), typeof(IHandpayValidator) };

        /// <inheritdoc />
        public bool HostOnline { get; private set; }

        /// <inheritdoc />
        public int IdReaderId
        {
            get => _properties.GetValue(AccountingConstants.IdReaderId, 0);

            set => _properties.SetProperty(AccountingConstants.IdReaderId, value);
        }

        /// <inheritdoc />
        public bool UsePlayerIdReader
        {
            get => _properties.GetValue(AccountingConstants.UsePlayerIdReader, false);

            set => _properties.SetProperty(AccountingConstants.UsePlayerIdReader, value);
        }

        /// <inheritdoc />
        public bool EnableReceipts
        {
            get => _properties.GetValue(AccountingConstants.EnableReceipts, false);

            set => _properties.SetProperty(AccountingConstants.EnableReceipts, value);
        }

        /// <inheritdoc />
        public bool EnabledLocalHandpay
        {
            get => _properties.GetValue(AccountingConstants.EnabledLocalHandpay, true);

            set => _properties.SetProperty(AccountingConstants.EnabledLocalHandpay, value);
        }

        /// <inheritdoc />
        public bool EnabledLocalCredit
        {
            get => _properties.GetValue(AccountingConstants.EnabledLocalCredit, true);

            set => _properties.SetProperty(AccountingConstants.EnabledLocalCredit, value);
        }

        /// <inheritdoc />
        public bool EnabledLocalVoucher
        {
            get => _properties.GetValue(AccountingConstants.EnabledLocalVoucher, true);

            set => _properties.SetProperty(AccountingConstants.EnabledLocalVoucher, value);
        }

        /// <inheritdoc />
        public bool EnabledLocalWat
        {
            get => _properties.GetValue(AccountingConstants.EnabledLocalWat, true);

            set => _properties.SetProperty(AccountingConstants.EnabledLocalWat, value);
        }

        /// <inheritdoc />
        public bool EnabledRemoteHandpay
        {
            get => _properties.GetValue(AccountingConstants.EnabledRemoteHandpay, false);

            set => _properties.SetProperty(AccountingConstants.EnabledRemoteHandpay, value);
        }

        /// <inheritdoc />
        public bool EnabledRemoteCredit
        {
            get => _properties.GetValue(AccountingConstants.EnabledRemoteCredit, false);

            set => _properties.SetProperty(AccountingConstants.EnabledRemoteCredit, value);
        }

        /// <inheritdoc />
        public bool EnabledRemoteVoucher
        {
            get => _properties.GetValue(AccountingConstants.EnabledRemoteVoucher, false);

            set => _properties.SetProperty(AccountingConstants.EnabledRemoteVoucher, value);
        }

        /// <inheritdoc />
        public bool EnabledRemoteWat
        {
            get => _properties.GetValue(AccountingConstants.EnabledRemoteWat, false);

            set => _properties.SetProperty(AccountingConstants.EnabledRemoteWat, value);
        }

        /// <inheritdoc />
        public bool DisabledLocalHandpay
        {
            get => _properties.GetValue(AccountingConstants.DisabledLocalHandpay, true);

            set => _properties.SetProperty(AccountingConstants.DisabledLocalHandpay, value);
        }

        /// <inheritdoc />
        public bool DisabledLocalCredit
        {
            get => _properties.GetValue(AccountingConstants.DisabledLocalCredit, false);

            set => _properties.SetProperty(AccountingConstants.DisabledLocalCredit, value);
        }

        /// <inheritdoc />
        public bool DisabledLocalVoucher
        {
            get => _properties.GetValue(AccountingConstants.DisabledLocalVoucher, false);

            set => _properties.SetProperty(AccountingConstants.DisabledLocalVoucher, value);
        }

        /// <inheritdoc />
        public bool DisabledLocalWat
        {
            get => _properties.GetValue(AccountingConstants.DisabledLocalWat, false);

            set => _properties.SetProperty(AccountingConstants.DisabledLocalWat, value);
        }

        /// <inheritdoc />
        public bool DisabledRemoteHandpay
        {
            get => _properties.GetValue(AccountingConstants.DisabledRemoteHandpay, false);

            set => _properties.SetProperty(AccountingConstants.DisabledRemoteHandpay, value);
        }

        /// <inheritdoc />
        public bool DisabledRemoteCredit
        {
            get => _properties.GetValue(AccountingConstants.DisabledRemoteCredit, false);

            set => _properties.SetProperty(AccountingConstants.DisabledRemoteCredit, value);
        }

        /// <inheritdoc />
        public bool DisabledRemoteVoucher
        {
            get => _properties.GetValue(AccountingConstants.DisabledRemoteVoucher, false);

            set => _properties.SetProperty(AccountingConstants.DisabledRemoteVoucher, value);
        }

        /// <inheritdoc />
        public bool DisabledRemoteWat
        {
            get => _properties.GetValue(AccountingConstants.DisabledRemoteWat, false);

            set => _properties.SetProperty(AccountingConstants.DisabledRemoteWat, value);
        }

        /// <inheritdoc />
        public LocalKeyOff LocalKeyOff
        {
            get => _properties.GetValue(AccountingConstants.LocalKeyOff, LocalKeyOff.AnyKeyOff);

            set => _properties.SetProperty(AccountingConstants.LocalKeyOff, value);
        }

        /// <inheritdoc />
        public bool PartialHandpays
        {
            get => _properties.GetValue(AccountingConstants.PartialHandpays, false);

            set => _properties.SetProperty(AccountingConstants.PartialHandpays, value);
        }

        /// <inheritdoc />
        public bool MixCreditTypes
        {
            get => _properties.GetValue(AccountingConstants.MixCreditTypes, true);

            set => _properties.SetProperty(AccountingConstants.MixCreditTypes, value);
        }

        /// <inheritdoc />
        public bool RequestNonCash
        {
            get => _properties.GetValue(AccountingConstants.RequestNonCash, false);

            set => _properties.SetProperty(AccountingConstants.RequestNonCash, value);
        }

        /// <inheritdoc />
        public bool CombineCashableOut
        {
            get => _properties.GetValue(AccountingConstants.CombineCashableOut, true);

            set => _properties.SetProperty(AccountingConstants.CombineCashableOut, value);
        }

        /// <inheritdoc />
        public string TitleJackpotReceipt
        {
            get => string.IsNullOrEmpty(_properties.GetValue(AccountingConstants.TitleJackpotReceipt, string.Empty))
                ? Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.JackpotHandpayTicket)
                : _properties.GetValue(AccountingConstants.TitleJackpotReceipt, string.Empty);

            set => _properties.SetProperty(AccountingConstants.TitleJackpotReceipt, value);
        }

        /// <inheritdoc />
        public string TitleCancelReceipt
        {
            get => string.IsNullOrEmpty(_properties.GetValue(AccountingConstants.TitleCancelReceipt, string.Empty))
                ? Localizer.For(CultureFor.PlayerTicket).GetString(ResourceKeys.HandpayReceipt)
                : _properties.GetValue(AccountingConstants.TitleCancelReceipt, string.Empty);

            set => _properties.SetProperty(AccountingConstants.TitleCancelReceipt, value);
        }

        public bool AllowLocalHandpay
        {
            get
            {
                var device = _egm.GetDevice<IHandpayDevice>();
                if (device == null)
                {
                    return false;
                }

                if (device.HostEnabled)
                {
                    if (!EnabledLocalHandpay)
                    {
                        return false;
                    }

                    // todo additional logic to determine if Local handpay is allowed when device is enabled

                    return true;
                }

                if (!DisabledLocalHandpay)
                {
                    return false;
                }

                // todo additional logic to determine if Local handpay is allowed when device is disabled

                return true;
            }
        }

        /// <inheritdoc />
        public bool AllowLocalVoucher
        {
            get
            {
                var device = _egm.GetDevice<IHandpayDevice>();
                if (device == null)
                {
                    return false;
                }

                if (device.HostEnabled)
                {
                    if (!EnabledLocalVoucher)
                    {
                        return false;
                    }

                    // todo additional logic to determine if Local voucher handpay is allowed when device is enabled

                    return true;
                }

                if (!DisabledLocalVoucher)
                {
                    return false;
                }

                // todo additional logic to determine if Local voucher handpay is allowed when device is disabled

                return true;
            }
        }

        /// <inheritdoc />
        public bool AllowLocalCredit
        {
            get
            {
                var device = _egm.GetDevice<IHandpayDevice>();
                if (device == null)
                {
                    return false;
                }

                if (device.HostEnabled)
                {
                    if (!EnabledLocalCredit)
                    {
                        return false;
                    }

                    // todo additional logic to determine if Local credit handpay is allowed when device is enabled

                    return true;
                }

                if (!DisabledLocalCredit)
                {
                    return false;
                }

                // todo additional logic to determine if Local credit handpay is allowed when device is disabled

                return true;
            }
        }

        /// <inheritdoc />
        public bool AllowLocalWat
        {
            get
            {
                var device = _egm.GetDevice<IHandpayDevice>();
                if (device == null)
                {
                    return false;
                }

                if (device.HostEnabled)
                {
                    if (!EnabledLocalWat)
                    {
                        return false;
                    }

                    // todo additional logic to determine if Local wat handpay is allowed when device is enabled

                    return true;
                }

                if (!DisabledLocalWat)
                {
                    return false;
                }

                // todo additional logic to determine if Local wat handpay is allowed when device is disabled

                return true;
            }
        }

        /// <inheritdoc />
        public bool AllowRemoteHandpay
        {
            get
            {
                 //if operator doesn't allow remote handpay then return false
                if (!_properties.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true))
                {
                    return false;
                }

                var device = _egm.GetDevice<IHandpayDevice>();
                if (device == null)
                {
                    return false;
                }

                if (device.HostEnabled)
                {
                    if (!EnabledRemoteHandpay)
                    {
                        return false;
                    }

                    // todo additional logic to determine if Remote handpay is allowed when device is enabled

                    return true;
                }

                if (!DisabledRemoteHandpay)
                {
                    return false;
                }

                // todo additional logic to determine if Remote handpay is allowed when device is disabled

                return true;
            }
        }

        /// <inheritdoc />
        public bool AllowRemoteVoucher
        {
            get
            {
                 //if operator doesn't allow remote handpay then return false
                if (!_properties.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true))
                {
                    return false;
                }
                var device = _egm.GetDevice<IHandpayDevice>();
                if (device == null)
                {
                    return false;
                }

                if (device.HostEnabled)
                {
                    if (!EnabledRemoteVoucher)
                    {
                        return false;
                    }

                    // todo additional logic to determine if Remote voucher handpay is allowed when device is enabled

                    return true;
                }

                if (!DisabledRemoteVoucher)
                {
                    return false;
                }

                // todo additional logic to determine if Remote voucher handpay is allowed when device is disabled

                return true;
            }
        }

        /// <inheritdoc />
        public bool AllowRemoteCredit
        {
            get
            {
                //if operator doesn't allow remote handpay then return false
                if (!_properties.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true))
                {
                    return false;
                }
                var device = _egm.GetDevice<IHandpayDevice>();
                if (device == null)
                {
                    return false;
                }

                if (device.HostEnabled)
                {
                    if (!EnabledRemoteCredit)
                    {
                        return false;
                    }

                    // todo additional logic to determine if Remote credit handpay is allowed when device is enabled

                    return true;
                }

                if (!DisabledRemoteCredit)
                {
                    return false;
                }

                // todo additional logic to determine if Remote credit handpay is allowed when device is disabled

                return true;
            }
        }

        /// <inheritdoc />
        public bool AllowRemoteWat
        {
            get
            {
                //if operator doesn't allow remote handpay then return false
                if (!_properties.GetValue(AccountingConstants.RemoteHandpayResetAllowed, true))
                {
                    return false;
                }
                var device = _egm.GetDevice<IHandpayDevice>();
                if (device == null)
                {
                    return false;
                }

                if (device.HostEnabled)
                {
                    if (!EnabledRemoteWat)
                    {
                        return false;
                    }

                    // todo additional logic to determine if Remote wat handpay is allowed when device is enabled

                    return true;
                }

                if (!DisabledRemoteWat)
                {
                    return false;
                }

                // todo additional logic to determine if Remote wat handpay is allowed when device is disabled

                return true;
            }
        }

        /// <inheritdoc />
        public bool LogTransactionRequired(Accounting.Contracts.ITransaction _)
        {
            return (bool)_properties.GetProperty(AccountingConstants.ValidateHandpays, false);
        }

        /// <inheritdoc />
        public void Initialize()
        {
            Logger.Info("Initializing the G2S HandpayService.");

            SubscribeToEvents();

            foreach (var transaction in _transactionHistory.RecallTransactions<HandpayTransaction>()
                .Where(x => x.State > HandpayState.Requested && x.State < HandpayState.Acknowledged))
            {
                _retryCommandsQueue.Post(transaction.TransactionId);
            }
        }

        public bool ValidateHandpay(
            long cashableAmount,
            long promoAmount,
            long nonCashAmount,
            HandpayType handpayType)
        {
            // Stop the handpay if not configured to support non-cashable handpays and there is no cashable nor promo amount.
            return _properties.GetValue(AccountingConstants.RequestNonCash, false) || cashableAmount + promoAmount > 0 || handpayType == HandpayType.BonusPay;
        }

        /// <inheritdoc />
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
                _eventBus.UnsubscribeAll(this);
            }

            _disposed = true;
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<CommunicationsStateChangedEvent>(this, Handle);
            _eventBus.Subscribe<HandpayKeyedOffEvent>(this, Handle);
            _eventBus.Subscribe<HandpayCanceledEvent>(this, Handle);
        }

        public async Task RequestHandpay(HandpayTransaction transaction)
        {
            transaction.PrintTicket = !string.IsNullOrWhiteSpace(transaction.Barcode) || _properties.GetValue(ApplicationConstants.HandpayReceiptPrintingEnabled, true);

            var handpay = _egm.GetDevice<IHandpayDevice>();
            if (handpay == null)
            {
                transaction.State = HandpayState.Pending;
                return;
            }

            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                transaction.State = HandpayState.Pending;
                return;
            }

            if (cabinet.Device != handpay)
            {
                cabinet.AddCondition(handpay, EgmState.EgmLocked);
            }

            var status = new handpayStatus();
            await _statusCommandBuilder.Build(handpay, status);

            _eventLift.Report(
                handpay,
                EventCode.G2S_JPE101,
                handpay.DeviceList(status),
                transaction.TransactionId,
                handpay.TransactionList(transaction.GetLog(this, _references)),
                null);

            // TODO:  This is updated in the provider, but we need to set it here before the event is emitted.
            //  Using the HandpayKeyOffPendingEvent introduces timing issues when the handpay is forcibly keyed off
            transaction.State = HandpayState.Pending;
            transaction.HostOnline = HostOnline;

            if (!await SendRequest(transaction))
            {
                await _retryCommandsQueue.SendAsync(transaction.TransactionId);
            }
        }

        private async Task OnKeyedOff(HandpayTransaction transaction)
        {
            var handpay = _egm.GetDevice<IHandpayDevice>();
            if (handpay == null)
            {
                return;
            }

            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                return;
            }

            // Set whether or not the host is offline (may have gone offline while waiting for key-off). 
            transaction.HostOnline = HostOnline;

            cabinet.RemoveCondition(handpay, EgmState.EgmLocked);

            var status = new handpayStatus();
            await _statusCommandBuilder.Build(handpay, status);

            var meters = new meterList
            {
                meterInfo =
                    _handpayMeters.GetMeters(
                            handpay,
                            TransferMeterName.CashableOutAmount,
                            TransferMeterName.PromoOutAmount,
                            TransferMeterName.NonCashableOutAmount,
                            TransferMeterName.TransferOutCount
                        )
                        .Concat(
                            _cabinetMeters.GetMeters(
                                cabinet,
                                CabinetMeterName.PlayerCashableAmount,
                                CabinetMeterName.PlayerPromoAmount,
                                CabinetMeterName.PlayerNonCashableAmount,
                                CabinetMeterName.HandPaidCancelAmount
                            )
                        )
                        .ToArray()
            };

            var transactionList = handpay.TransactionList(transaction.GetLog(this, _references));

            AppendTransactions(transactionList, transaction);

            _eventLift.Report(
                handpay,
                EventCode.G2S_JPE104,
                handpay.DeviceList(status),
                transaction.TransactionId,
                transactionList,
                meters);

            if (!await SendKeyedOff(transaction))
            {
                await _retryCommandsQueue.SendAsync(transaction.TransactionId);
            }
        }

        private async Task OnCancelled(HandpayTransaction transaction)
        {
            var handpay = _egm.GetDevice<IHandpayDevice>();
            if (handpay == null)
            {
                return;
            }

            var cabinet = _egm.GetDevice<ICabinetDevice>();
            if (cabinet == null)
            {
                return;
            }

            cabinet.RemoveCondition(handpay, EgmState.EgmLocked);

            if (transaction.HandpayType == HandpayType.CancelCredit)
            {
                var status = new handpayStatus();
                await _statusCommandBuilder.Build(handpay, status);

                _eventLift.Report(
                    handpay,
                    EventCode.G2S_JPE106,
                    handpay.DeviceList(status),
                    transaction.TransactionId,
                    handpay.TransactionList(transaction.GetLog(this, _references)),
                    null);
            }
        }

        private async Task Handle(CommunicationsStateChangedEvent theEvent, CancellationToken cancellation)
        {
            var device = _egm.GetDevice<IHandpayDevice>();

            if (theEvent.HostId != device?.Owner)
            {
                return;
            }

            HostOnline = theEvent.Online;

            await Task.CompletedTask;
        }

        private async Task Handle(HandpayCanceledEvent theEvent, CancellationToken cancellation)
        {
            await OnCancelled(theEvent.Transaction);
        }

        private async Task Handle(HandpayKeyedOffEvent theEvent, CancellationToken cancellation)
        {
            await OnKeyedOff(theEvent.Transaction);
        }

        private void AppendTransactions(transactionList transactionList, HandpayTransaction transaction)
        {
            if (!transaction.AssociatedTransactions.Any())
            {
                return;
            }

            var transactions = _transactionHistory.RecallTransactions()
                .Where(
                    t => t is ITransactionConnector connector &&
                         connector.AssociatedTransactions.Contains(transaction.TransactionId)).ToList();

            var info = new List<transactionInfo>();

            foreach (var associatedTransaction in transactions)
            {
                switch (associatedTransaction)
                {
                    case VoucherOutTransaction voucher:
                        var voucherDevice = _egm.GetDevice<IVoucherDevice>();
                        if (voucherDevice != null)
                        {
                            info.Add(new transactionInfo
                            {
                                deviceClass = voucherDevice.PrefixedDeviceClass(),
                                deviceId = voucherDevice.Id,
                                Item = voucher.ToLog(_references)
                            });
                        }
                        break;
                }
            }

            transactionList.transactionInfo = transactionList.transactionInfo.Concat(info).ToArray();
        }

        private async Task<bool> SendRequest(HandpayTransaction transaction)
        {
            var handpay = _egm.GetDevice<IHandpayDevice>();
            if (handpay == null)
            {
                return false;
            }

            var command = new handpayRequest
            {
                transactionId = transaction.TransactionId,
                handpayType = transaction.HandpayType.ToG2SEnum(),
                handpayDateTime = transaction.TransactionDateTime,
                requestCashableAmt = transaction.CashableAmount,
                requestPromoAmt = transaction.PromoAmount,
                requestNonCashAmt = transaction.NonCashAmount,
                egmPaidCashableAmt = 0,
                egmPaidPromoAmt = 0,
                egmPaidNonCashAmt = 0,
                idReaderType = "G2S_none",
                idNumber = string.Empty,
                playerId = string.Empty,
                localHandpay = AllowLocalHandpay,
                localCredit = AllowLocalCredit,
                localVoucher = AllowLocalVoucher,
                localWat = AllowLocalWat,
                remoteHandpay = AllowRemoteHandpay,
                remoteCredit = AllowRemoteCredit,
                remoteVoucher = AllowRemoteVoucher,
                remoteWat = AllowRemoteWat,
                handpaySourceRef = _references.GetReferences<handpaySourceRef>(transaction).ToArray()
            };

            if (!await handpay.Request(command))
            {
                return false;
            }

            transaction.RequestAcknowledged = true;
            _transactionHistory.UpdateTransaction(transaction);

            _eventLift.Report(
                handpay,
                EventCode.G2S_JPE102,
                transaction.TransactionId,
                handpay.TransactionList(transaction.GetLog(this, _references)));

            return true;
        }

        private async Task<bool> SendKeyedOff(HandpayTransaction transaction)
        {
            var handpay = _egm.GetDevice<IHandpayDevice>();
            if (handpay == null)
            {
                return false;
            }

            var command = new keyedOff
            {
                transactionId = transaction.TransactionId,
                keyOffType = transaction.KeyOffType.ToG2SEnum(),
                keyOffCashableAmt = transaction.KeyOffCashableAmount,
                keyOffPromoAmt = transaction.KeyOffPromoAmount,
                keyOffNonCashAmt = transaction.KeyOffNonCashAmount,
                keyOffDateTime = transaction.KeyOffDateTime
            };

            if (!await handpay.KeyedOff(command))
            {
                return false;
            }

            transaction.State = HandpayState.Acknowledged;
            _transactionHistory.UpdateTransaction(transaction);

            var status = new handpayStatus();
            await _statusCommandBuilder.Build(handpay, status);

            _eventLift.Report(
                handpay,
                EventCode.G2S_JPE105,
                transaction.TransactionId,
                handpay.TransactionList(transaction.GetLog(this, _references)));

            return true;
        }

        private async Task<bool> WaitForHostOnline(CancellationToken cancellation)
        {
            while (!HostOnline)
            {
                if (cancellation.WaitHandle.WaitOne(1000))
                {
                    return false;
                }
            }

            return await Task.FromResult(true);
        }

        private HandpayTransaction GetTransaction(long transactionId)
        {
            return _transactionHistory
                .RecallTransactions<HandpayTransaction>()
                .FirstOrDefault(x => x.TransactionId == transactionId);
        }

        private async Task RetryCommand(long transactionId)
        {
            if (GetTransaction(transactionId) == null)
            {
                Logger.Error($"Transaction {transactionId} was not found");
                // bail out and do not try to re-send for this transaction
                return;
            }

            if (GetTransaction(transactionId).State == HandpayState.Requested)
            {
                Logger.Warn(
                    $"Transaction {transactionId} has current state of Requested which means a request to the server has not been attempted yet.");
                return;
            }

            while (GetTransaction(transactionId).State != HandpayState.Acknowledged)
            {
                using (var cancellation = new CancellationTokenSource())
                {
                    cancellation.CancelAfter(TimeSpan.FromSeconds(RetryTimeout));

                    if (!await WaitForHostOnline(cancellation.Token))
                    {
                        continue;
                    }
                }

                var transaction = GetTransaction(transactionId);

                if (transaction.State > HandpayState.Requested && !transaction.RequestAcknowledged)
                {
                    if (!await SendRequest(transaction))
                    {
                        continue;
                    }
                }

                if (transaction.State == HandpayState.Committed)
                {
                    await SendKeyedOff(transaction);
                }

                // Short delay to allow normal transaction processing to take priority
                // and to keep from spamming the host
                await Task.Delay(TimeSpan.FromMilliseconds(500), CancellationToken.None);
            }
        }
    }
}
