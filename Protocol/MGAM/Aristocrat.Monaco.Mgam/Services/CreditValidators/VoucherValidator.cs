namespace Aristocrat.Monaco.Mgam.Services.CreditValidators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Accounting.Contracts.Vouchers;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Attributes;
    using Commands;
    using Common;
    using Common.Data.Models;
    using Common.Data.Repositories;
    using Common.Events;
    using Gaming.Contracts;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Kernel.Contracts;
    using Lockup;
    using Notification;
    using Protocol.Common.Storage.Entity;
    using BeginSession = Commands.BeginSession;
    using CreditVoucher = Commands.CreditVoucher;
    using EndSession = Commands.EndSession;
    using ValidateVoucher = Commands.ValidateVoucher;

    /// <summary>
    ///     Used to validate voucher requested by the EGM with the host.
    /// </summary>
    public class VoucherValidator : IVoucherValidator, IDisposable
    {
        private const long CreditCurrencyMultiplier = 1000;
        private const string Cash = "CASH";
        private const string Coupon = "COUPON";
        private const string Voucher = "VOUCHER";
        private readonly ILogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IPropertiesManager _properties;
        private readonly IAttributeManager _attributes;
        private readonly ICashOut _cashOutHandler;
        private readonly ICommandHandlerFactory _commandFactory;
        private readonly IEgm _egm;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILockup _lockoutHandler;
        private readonly INotificationLift _notificationLift;
        private readonly ITransactionHistory _transactionHistory;
        private readonly IGameHistory _gameHistory;
        private readonly IIdProvider _idProvider;
        private bool _reprintVoucher;
        private bool _clearVoucher;
        private bool _disposed;
        private long _sessionLimitMillicents;

        /// <summary>
        ///     Initializes a new instance of the <see cref="VoucherValidator" /> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="eventBus">Event bus.</param>
        /// <param name="properties">Properties manager.</param>
        /// <param name="attributes">Attributes manager.</param>
        /// <param name="cashOutHandler">Player cashOutHandler.</param>
        /// <param name="commandFactory">Command factory.</param>
        /// <param name="egm">EGM client.</param>
        /// <param name="unitOfWorkFactory">Unit of work factory.</param>
        /// <param name="lockoutHandler"><see cref="ILockup" />.</param>
        /// <param name="notificationLift"><see cref="INotificationLift" />.</param>
        /// <param name="transactionHistory"><see cref="ITransactionHistory" />.</param>
        /// <param name="gameHistory"><see cref="IGameHistory" />.</param>
        /// <param name="retryHandler"><see cref="ITransactionRetryHandler"/></param>
        /// <param name="idProvider"><see cref="IIdProvider"/></param>
        public VoucherValidator(
            ILogger<VoucherValidator> logger,
            IEventBus eventBus,
            IPropertiesManager properties,
            IAttributeManager attributes,
            ICashOut cashOutHandler,
            ICommandHandlerFactory commandFactory,
            IEgm egm,
            IUnitOfWorkFactory unitOfWorkFactory,
            ILockup lockoutHandler,
            INotificationLift notificationLift,
            ITransactionHistory transactionHistory,
            IGameHistory gameHistory,
            ITransactionRetryHandler retryHandler,
            IIdProvider idProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
            _cashOutHandler = cashOutHandler ?? throw new ArgumentNullException(nameof(cashOutHandler));
            _commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
            _egm = egm ?? throw new ArgumentNullException(nameof(egm));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _lockoutHandler = lockoutHandler ?? throw new ArgumentNullException(nameof(lockoutHandler));
            _notificationLift = notificationLift ?? throw new ArgumentNullException(nameof(notificationLift));
            _transactionHistory = transactionHistory ?? throw new ArgumentNullException(nameof(transactionHistory));
            _gameHistory = gameHistory ?? throw new ArgumentNullException(nameof(gameHistory));
            _idProvider = idProvider ?? throw new ArgumentNullException(nameof(idProvider));

            if (retryHandler == null)
            {
                throw new ArgumentNullException(nameof(retryHandler));
            }

            retryHandler.RegisterRetryAction(typeof(Aristocrat.Mgam.Client.Messaging.EndSession), TriggerReprintVoucher);

            SubscribeToEvents();
        }

        /// <summary>
        ///     Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public string Name => typeof(VoucherValidator).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IVoucherValidator) };

        /// <inheritdoc />
        public void Initialize()
        {
            _sessionLimitMillicents =
                ((long)_attributes.Get(AttributeNames.SessionBalanceLimit, MgamConstants.DefaultSessionBalanceLimit))
                .CentsToMillicents();

            if (!_gameHistory.IsRecoveryNeeded)
            {
                TriggerReprintVoucher();
            }
        }

        /// <inheritdoc />
        public bool HostOnline { get; private set; }

        /// <inheritdoc />
        public bool ReprintFailedVoucher => true;

        /// <inheritdoc />
        public bool CanValidateVouchersIn => true;

        /// <inheritdoc />
        public bool CanCombineCashableAmounts => true;

        /// <inheritdoc />
        public bool CanValidateVoucherOut(long amount, AccountType type)
        {
            return true;
        }

        /// <inheritdoc />
        public async Task<VoucherAmount> RedeemVoucher(VoucherInTransaction transaction)
        {
            VoucherAmount voucherAmount;

            try
            {
                _logger.LogInfo($"Sending Validate Voucher Barcode:{transaction.Barcode}");
                var voucherIn = new ValidateVoucher { Barcode = transaction.Barcode };
                await _commandFactory.Execute(voucherIn);

                voucherAmount = new VoucherAmount(
                    voucherIn.VoucherCashValue * CreditCurrencyMultiplier,
                    voucherIn.VoucherCouponValue * CreditCurrencyMultiplier,
                    0);
                transaction.Amount = voucherAmount.Amount;

                transaction.LogDisplayType = Common.VoucherExtensions.GetLogDisplayType(voucherAmount);

                if(_cashOutHandler.Balance >= _sessionLimitMillicents)
                {
                    _logger.LogInfo($"Voucher failed session balance limit check {_sessionLimitMillicents} balance {_cashOutHandler.Balance}");

                    transaction.Exception = (int)VoucherInExceptionCode.CreditLimitExceeded;

                    return null;
                }
            }
            catch (ServerResponseException response)
            {
                _logger.LogInfo($"Failed Validate Voucher Reason:{response.ResponseCode}");

                switch (response.ResponseCode)
                {
                    case ServerResponseCode.BarcodeNotFound:
                    case ServerResponseCode.InvalidBarcode:
                        transaction.Exception = (int)VoucherInExceptionCode.InvalidTicket;
                        break;
                    case ServerResponseCode.VoucherExpired:
                        transaction.Exception = (int)VoucherInExceptionCode.Expired;
                        break;
                    case ServerResponseCode.VoucherRedeemed:
                        transaction.Exception = (int)VoucherInExceptionCode.AlreadyReedemed;
                        break;
                    default:
                        transaction.Exception = (int)VoucherInExceptionCode.Other;
                        break;
                }

                return null;
            }

            try
            {
                bool beginSession;
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    beginSession = unitOfWork.Repository<Session>().GetSessionId() == null;
                }

                if (beginSession)
                {
                    _logger.LogInfo("Sending Begin Session");
                    await _commandFactory.Execute(new BeginSession());
                    using (var unitOfWork = _unitOfWorkFactory.Create())
                    {
                        if (unitOfWork.Repository<Session>().GetSessionId() == null)
                        {
                            transaction.Exception = (int)VoucherInExceptionCode.Other;

                            return null;
                        }
                    }
                }
            }
            catch (ServerResponseException response)
            {
                var errorMessage = $"Begin Session failed ResponseCode:{response.ResponseCode}";
                _logger.LogInfo(errorMessage);

                transaction.Exception = (int)VoucherInExceptionCode.Other;

                _lockoutHandler.LockupForEmployeeCard(errorMessage);

                return null;
            }

            return voucherAmount;
        }

        /// <inheritdoc />
        public async Task StackedVoucher(VoucherInTransaction transaction)
        {
            try
            {
                _logger.LogInfo($"Sending Credit Voucher Barcode:{transaction.Barcode}");
                await _commandFactory.Execute(new CreditVoucher { Barcode = transaction.Barcode });

                transaction.VoucherSequence = (int)_idProvider.GetNextLogSequence<VoucherInTransaction>();
            }
            catch (ServerResponseException)
            {
                if (HostOnline)
                {
                    using (var unitOfWork = _unitOfWorkFactory.Create())
                    {
                        var voucherData = new Voucher();
                        voucherData.Validate();
                        voucherData.OfflineReason = VoucherOutOfflineReason.Credit;

                        unitOfWork.Repository<Voucher>().AddVoucher(voucherData);

                        unitOfWork.SaveChanges();
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<VoucherOutTransaction> IssueVoucher(
            VoucherAmount amount,
            AccountType type,
            Guid transactionId,
            TransferOutReason reason)
        {
            Session session;

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                session = unitOfWork.Repository<Session>().Queryable().SingleOrDefault();
            }

            if ((!_reprintVoucher || session != null) &&
                reason == TransferOutReason.CashOut)
            {
                if (!HostOnline)
                {
                    CreateOfflineVoucher(amount);
                }
                else
                {
                    try
                    {
                        _logger.LogInfo($"Sending End Session Amount:{amount.Amount.MillicentsToDollars().FormattedCurrencyString()}");
                        await _commandFactory.Execute(
                            new EndSession { Balance = amount.Amount.MillicentsToDollars().FormattedCurrencyString() });
                    }
                    catch (ServerResponseException response)
                    {
                        _logger.LogInfo($"Failed End Session Reason:{response.ResponseCode}");

                        if (response.ResponseCode.Equals(ServerResponseCode.DoNotPrintVoucher))
                        {
                            // this will fall through to a handpay
                            DeleteVoucher();
                            return await Task.FromResult<VoucherOutTransaction>(null);
                        }

                        _eventBus.Publish(new ForceDisconnectEvent(DisconnectReason.InvalidServerResponse));

                        CreateOfflineVoucher(amount);
                    }
                }
            }

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var voucher = unitOfWork.Repository<Voucher>().Queryable().SingleOrDefault();
                if (voucher != null)
                {
                    // don't delete incomplete vouchers
                    var transaction = voucher.ToTransaction(amount.Amount, type, reason);
                    if (transaction == null)
                    {
                        // we could be re-connecting, and the call to end session was aborted early
                        voucher = CreateOfflineVoucher(amount);
                        transaction = voucher?.ToTransaction(amount.Amount, type, reason);

                        if (transaction == null)
                        {
                            // we need to call end session again or create an offline voucher
                            _reprintVoucher = false;
                            _logger.LogInfo("Failed retrieve voucher data incomplete");
                        }
                    }

                    if (transaction != null)
                    {
                        transaction.LogDisplayType = Common.VoucherExtensions.GetLogDisplayType(amount);
                    }

                    return transaction;
                }
            }

            if (session == null)
            {
                _logger.LogError(
                    $"No Active Session {amount.Amount.MillicentsToDollars().FormattedCurrencyString()} Voucher Out Failed");
                _lockoutHandler.LockupForEmployeeCard(
                    $"No Active Session {amount.Amount.MillicentsToDollars().FormattedCurrencyString()} Voucher Out Failed");
            }

            return await Task.FromResult<VoucherOutTransaction>(null);
        }

        /// <inheritdoc />
        public void CommitVoucher(VoucherInTransaction transaction)
        {
            transaction.CommitAcknowledged = true;

            _transactionHistory.UpdateTransaction(transaction);
        }

        private Voucher CreateOfflineVoucher(VoucherAmount amount)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var sessionRepository = unitOfWork.Repository<Session>();
                var voucherRepository = unitOfWork.Repository<Voucher>();

                var session = sessionRepository.Queryable().SingleOrDefault();
                if (session != null && !session.OfflineVoucherPrinted)
                {
                    var totalLongForm =
                        CurrencyExtensions.ConvertCurrencyToWords(amount.Amount.MillicentsToDollars());

                    var voucher = voucherRepository.Queryable().SingleOrDefault();
                    if (voucher == null)
                    {
                        voucher = new Voucher();
                        voucher.Validate();
                        voucher.OfflineReason = VoucherOutOfflineReason.Cashout;
                    }

                    voucher.VoucherBarcode = session.OfflineVoucherBarcode;
                    voucher.AmountLongForm = totalLongForm;
                    voucher.CashAmount = $"{Cash}: {amount.CashAmount.MillicentsToDollars().FormattedCurrencyString()}";
                    voucher.CouponAmount =
                        $"{Coupon}: {amount.PromoAmount.MillicentsToDollars().FormattedCurrencyString()}";
                    voucher.CasinoAddress =
                        _attributes.Get(AttributeNames.LocationAddress, string.Empty);
                    voucher.CasinoName =
                        _attributes.Get(AttributeNames.LocationName, string.Empty);
                    voucher.Expiration =
                        _attributes.Get(AttributeNames.VoucherExpiration, string.Empty);
                    voucher.Date = DateTime.Now.ToString(MgamConstants.DateFormat);
                    voucher.Time = DateTime.Now.ToString(MgamConstants.TimeFormat);
                    voucher.DeviceId =
                        $"{_egm.ActiveInstance?.DeviceId ?? 0} {session.SessionId} {_egm.ActiveInstance?.SiteId ?? 0}";
                    voucher.TotalAmount = amount.Amount.MillicentsToDollars().FormattedCurrencyString();
                    var voucherType = amount.PromoAmount != 0 && amount.CashAmount != 0
                        ? $"{Cash}/{Coupon}"
                        : amount.CashAmount != 0
                            ? Cash
                            : Coupon;
                    voucher.VoucherType = $"{voucherType} {Voucher}";

                    session.OfflineVoucherPrinted = true;

                    sessionRepository.Update(session);

                    voucherRepository.AddVoucher(voucher);

                    unitOfWork.SaveChanges();

                    return voucher;
                }
            }

            return null;
        }

        private void Dispose(bool disposing)
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
            _eventBus.Subscribe<SoftwareUpgradeStartedEvent>(this, _ => _cashOutHandler.CashOut());
            _eventBus.Subscribe<LockupResolvedEvent>(this, HandleEvent);
            _eventBus.Subscribe<TransferOutFailedEvent>(this, HandleEvent);
            _eventBus.Subscribe<VoucherIssuedEvent>(this, HandleEvent);
            _eventBus.Subscribe<EnabledEvent>(this, HandleEvent);
            _eventBus.Subscribe<ReadyToPlayEvent>(this, HandleEvent);
            _eventBus.Subscribe<HostOfflineEvent>(this, HandleEvent);
            _eventBus.Subscribe<AttributeChangedEvent>(this, e => UpdateAttribute(e.AttributeName));
            _eventBus.Subscribe<AttributesUpdatedEvent>(this, HandleEvent);
            _eventBus.Subscribe<PrimaryGameFailedEvent>(this, _ => _clearVoucher = true);
            _eventBus.Subscribe<GamePlayInitiatedEvent>(this, _ =>
            {
                if (!_clearVoucher)
                {
                    return;
                }

                DeleteVoucher();
                _clearVoucher = false;
            });
        }

        private void HandleEvent(HostOfflineEvent @event)
        {
            HostOnline = false;
        }

        private void HandleEvent(ReadyToPlayEvent @event)
        {
            HostOnline = true;

            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var voucher = unitOfWork.Repository<Voucher>().Queryable().SingleOrDefault();
                if (voucher != null && !voucher.CanPrint() && voucher.OfflineReason != VoucherOutOfflineReason.None)
                {
                    voucher.OfflineReason = VoucherOutOfflineReason.None;

                    unitOfWork.Repository<Voucher>().Update(voucher);

                    unitOfWork.SaveChanges();
                }
            }
        }

        private void HandleEvent(EnabledEvent @event)
        {
            if (_reprintVoucher && !_lockoutHandler.IsLockedForEmployeeCard)
            {
                _cashOutHandler.CashOut();
            }
        }

        private void HandleEvent(LockupResolvedEvent @event)
        {
            if (!_lockoutHandler.IsLockedForEmployeeCard)
            {
                if (_reprintVoucher)
                {
                    _cashOutHandler.CashOut();
                }
            }
        }

        private void HandleEvent(TransferOutFailedEvent @event)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var voucher = unitOfWork.Repository<Voucher>().Queryable().SingleOrDefault();
                if (voucher != null)
                {
                    _reprintVoucher = true;

                    _lockoutHandler.LockupForEmployeeCard($"Failed to print voucher {voucher.VoucherBarcode}");

                    _notificationLift.Notify(
                        NotificationCode.LockedPrintVoucherFailed,
                        voucher.VoucherBarcode);
                }
            }
        }

        private void HandleEvent(VoucherIssuedEvent @event)
        {
            DeleteVoucher();

            _reprintVoucher = false;
        }

        private void DeleteVoucher()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var voucher = unitOfWork.Repository<Voucher>().Queryable().SingleOrDefault();
                if (voucher != null)
                {
                    unitOfWork.Repository<Voucher>().Delete(voucher);

                    unitOfWork.SaveChanges();
                }
            }
        }

        private void HandleEvent(AttributesUpdatedEvent e)
        {
            UpdateAttribute(AttributeNames.LocationName);
            UpdateAttribute(AttributeNames.LocationAddress);
        }

        private void UpdateAttribute(string attributeName)
        {
            switch (attributeName)
            {
                case AttributeNames.LocationName:
                case AttributeNames.LocationAddress:
                    var value = _attributes.Get(attributeName, string.Empty);
                    var key = attributeName == AttributeNames.LocationName
                        ? PropertyKey.TicketTextLine1
                        : PropertyKey.TicketTextLine2;
                    _properties.SetProperty(key, value);
                    break;
                case AttributeNames.SessionBalanceLimit:
                    _sessionLimitMillicents =
                        ((long)_attributes.Get(attributeName, MgamConstants.DefaultSessionBalanceLimit))
                        .CentsToMillicents();
                    _properties.SetProperty(AccountingConstants.MaxTenderInLimit, _sessionLimitMillicents);
                    break;
            }
        }

        private void TriggerReprintVoucher()
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                var voucher = unitOfWork.Repository<Voucher>().Queryable().SingleOrDefault();
                if (voucher != null && voucher.CanPrint())
                {
                    _reprintVoucher = true;
                    _cashOutHandler.CashOut();
                }
            }
        }
    }
}