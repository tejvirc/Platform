namespace Aristocrat.Monaco.Mgam.Services.CreditValidators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Application.Contracts.Identification;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Attributes;
    using Common;
    using Common.Data.Models;
    using Common.Events;
    using Hardware.Contracts.Button;
    using Kernel;
    using Lockup;
    using Notification;
    using Protocol.Common.Storage.Entity;

    /// <summary>
    ///     Used to validate handpays requested by the EGM with the host.
    /// </summary>
    public class HandpayValidator : IHandpayValidator
    {
        private const string ThresholdLimitExceeded = "Threshold Limit Exceeded";

        private static readonly int JackpotKeyDown = (int)ButtonLogicalId.Button30;
        private readonly ILogger<HandpayValidator> _logger;
        private readonly IPropertiesManager _properties;
        private readonly INotificationLift _notification;
        private readonly IEventBus _bus;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILockup _lockoutHandler;
        private readonly IAttributeManager _attributes;
        private long _securityLimit;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandpayValidator" /> class.
        /// </summary>
        public HandpayValidator(
            ILogger<HandpayValidator> logger,
            IPropertiesManager properties,
            INotificationLift notification,
            IEventBus bus,
            IUnitOfWorkFactory unitOfWorkFactory,
            ILockup lockoutHandler,
            IAttributeManager attributes)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _properties = properties ?? throw new ArgumentNullException(nameof(properties));
            _notification = notification ?? throw new ArgumentNullException(nameof(notification));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
            _lockoutHandler = lockoutHandler ?? throw new ArgumentNullException(nameof(lockoutHandler));
            _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));

            _properties.SetProperty(AccountingConstants.RequestNonCash, true);
            _bus.Subscribe<AttributeChangedEvent>(this, HandleEvent);
            _bus.Subscribe<HandpayStartedEvent>(this, _ => _bus.Subscribe<EmployeeLoggedInEvent>(this, Handle));
            _bus.Subscribe<HandpayCompletedEvent>(this, _ => _bus.Unsubscribe<EmployeeLoggedInEvent>(this));
        }

        /// <inheritdoc />
        public string Name => typeof(HandpayValidator).FullName;

        /// <inheritdoc />
        public ICollection<Type> ServiceTypes => new[] { typeof(IHandpayValidator) };

        /// <inheritdoc />
        public bool AllowLocalHandpay => true;

        /// <inheritdoc />
        public bool HostOnline => true;

        /// <inheritdoc />
        public bool LogTransactionRequired(ITransaction _)
        {
            return (bool)_properties.GetProperty(AccountingConstants.ValidateHandpays, false);
        }

        /// <inheritdoc />
        public void Initialize()
        {
        }

        /// <inheritdoc />
        public bool ValidateHandpay(long cashableAmount, long promoAmount, long nonCashAmount, HandpayType handpayType)
        {
            _properties.SetProperty(AccountingConstants.EnableReceipts, handpayType != HandpayType.CancelCredit);

            if (handpayType == HandpayType.CancelCredit)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var voucher = unitOfWork.Repository<Voucher>().Queryable().SingleOrDefault();
                    if (voucher != null)
                    {
                        _logger.LogError("Pending a voucher reprint.");
                        return false;
                    }
                }

                _logger.LogError("Handpay cancel credits should not happen, we are out of sync with the host");
                return true;
            }

            _logger.LogInfo(
                $"Validate Handpay cashableAmount: {cashableAmount} promoAmount: {promoAmount} nonCashAmount: {nonCashAmount} handpayType: {handpayType}");

            _properties.SetProperty(AccountingConstants.HandpayLargeWinForcedKeyOff, false);

            var totalAmount = (cashableAmount + promoAmount + nonCashAmount).MillicentsToCents();

            if (_securityLimit == 0)
            {
                SetSecurityLimit();
            }

            if (handpayType == HandpayType.GameWin &&
                _securityLimit <= totalAmount)
            {
                _lockoutHandler.LockupForEmployeeCard(ThresholdLimitExceeded);

                _notification.Notify(
                    NotificationCode.LockedWinThresholdExceeded,
                    totalAmount.ToString());
            }
            else
            {
                _properties.SetProperty(AccountingConstants.HandpayLargeWinForcedKeyOff, true);
                _properties.SetProperty(AccountingConstants.HandpayLargeWinKeyOffStrategy, KeyOffType.LocalHandpay);
            }

            return true;
        }

        /// <inheritdoc />
        public async Task RequestHandpay(HandpayTransaction transaction)
        {
            _logger.LogInfo($"RequestHandpay {transaction}");
            transaction.PrintTicket = transaction.HandpayType != HandpayType.CancelCredit;

            if (transaction.PrintTicket)
            {
                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var voucher = unitOfWork.Repository<Voucher>().Queryable().SingleOrDefault();
                    if (voucher != null)
                    {
                        transaction.TicketData = voucher.VoucherTicketData();
                        transaction.Barcode = voucher.VoucherBarcode;

                        unitOfWork.Repository<Voucher>().Delete(voucher);

                        unitOfWork.SaveChanges();
                    }
                }

                transaction.KeyOffType = KeyOffType.LocalHandpay;
            }

            await Task.CompletedTask;
        }

        private void Handle(EmployeeLoggedInEvent evt)
        {
            _bus.Publish(new DownEvent(JackpotKeyDown));
        }

        private void HandleEvent(AttributeChangedEvent @event)
        {
            switch (@event.AttributeName)
            {
                case AttributeNames.VoucherSecurityLimit:
                    SetSecurityLimit();
                    break;
            }
        }

        private void SetSecurityLimit()
        {
            _securityLimit = _attributes.Get(
                AttributeNames.VoucherSecurityLimit,
                MgamConstants.DefaultVoucherSecurityLimit);
        }
    }
}