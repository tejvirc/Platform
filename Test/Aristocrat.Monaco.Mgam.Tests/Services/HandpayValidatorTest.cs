namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using System;
    using System.Globalization;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Application.Contracts.Extensions;
    using Application.Contracts.Identification;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Monaco.Mgam.Services.Attributes;
    using Aristocrat.Monaco.Mgam.Services.Lockup;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Common;
    using Common.Data.Models;
    using Kernel;
    using Mgam.Services.CreditValidators;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class HandpayValidatorTest
    {
        private Mock<ILogger<HandpayValidator>> _logger;
        private Mock<IPropertiesManager> _properties;
        private Mock<INotificationLift> _notification;
        private Mock<IEventBus> _bus;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
        private Mock<ILockup> _lockoutHandler;
        private Mock<IAttributeManager> _attributes;

        private HandpayValidator _target;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            var factory = new Mock<ILocalizerFactory>();
            MoqServiceManager.Instance.Setup(m => m.GetService<ILocalizerFactory>()).Returns(factory.Object);
            factory.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);
            CurrencyExtensions.SetCultureInfo(CultureInfo.CurrentCulture, null, null, true, true, "c");

            _logger = new Mock<ILogger<HandpayValidator>>();
            _properties = new Mock<IPropertiesManager>();
            _properties.Setup(p => p.SetProperty(It.IsAny<string>(), It.IsAny<bool>()));
            _notification = new Mock<INotificationLift>();
            _bus = new Mock<IEventBus>();
            _lockoutHandler = new Mock<ILockup>();
            _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            _attributes = new Mock<IAttributeManager>();

            _target = new HandpayValidator(
                _logger.Object,
                _properties.Object,
                _notification.Object,
                _bus.Object,
                _unitOfWorkFactory.Object,
                _lockoutHandler.Object,
                _attributes.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullLoggerExpectException()
        {
            var service = new HandpayValidator(null, null,
                null, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullPropertiesExpectException()
        {
            var service = new HandpayValidator(_logger.Object, null,
                null, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullNotificationExpectException()
        {
            var service = new HandpayValidator(_logger.Object, _properties.Object,
                null, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullBusExpectException()
        {
            var service = new HandpayValidator(_logger.Object, _properties.Object,
                _notification.Object, null, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullUnitOfWorkFactoryExpectException()
        {
            var service = new HandpayValidator(_logger.Object, _properties.Object,
                _notification.Object, _bus.Object, null, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullLockHandlerExpectException()
        {
            var service = new HandpayValidator(_logger.Object, _properties.Object, 
                _notification.Object, _bus.Object, _unitOfWorkFactory.Object, null, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullAttributesExpectException()
        {
            var service = new HandpayValidator(_logger.Object, _properties.Object, 
                _notification.Object, _bus.Object, _unitOfWorkFactory.Object, _lockoutHandler.Object, null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            var service = new HandpayValidator(_logger.Object, _properties.Object,
                _notification.Object, _bus.Object, _unitOfWorkFactory.Object, _lockoutHandler.Object, _attributes.Object);

            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(IHandpayValidator));
        }

        [TestMethod]
        public void ValidateHandpayGameWinSuccess()
        {
            _bus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<EmployeeLoggedInEvent>>()));
            _notification.Setup(n => n.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>())).Verifiable();

            var result = _target.ValidateHandpay(1000, 0, 0, HandpayType.GameWin);
            
            _notification.Verify();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateHandpaySuccess()
        {
            var test = DataModelHelpers.SetUpDataModel(_unitOfWorkFactory, null as Voucher);
            test.repo.Setup(r => r.Delete(It.IsAny<Voucher>())).Verifiable();

            var result = _target.ValidateHandpay(1000, 0, 0, HandpayType.GameWin);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void RequestHandpaySuccessNotCancelCredit()
        {
            var voucher = new Voucher();
            voucher.Validate();
            var (_, repo, _) = DataModelHelpers.SetUpDataModel(_unitOfWorkFactory, voucher);
            repo.Setup(r => r.Delete(It.IsAny<Voucher>())).Verifiable();

            var transaction = new HandpayTransaction(1, DateTime.Today, 10000, 10000, 0, 100, HandpayType.GameWin, true, Guid.Empty);
            _target.RequestHandpay(transaction).Wait(10);

            repo.Verify();

            Assert.IsNotNull(transaction.TicketData);
        }

        /*
        /// <inheritdoc />
        public async Task RequestHandpay(HandpayTransaction transaction)
        {
            _logger.LogInfo($"RequestHandpay {transaction}");
            transaction.PrintTicket = _properties.GetValue(AccountingConstants.EnableReceipts, false);

            if (transaction.PrintTicket && transaction.HandpayType == HandpayType.CancelCredit)
            {
                SetTicketData(transaction, _unitOfWorkFactory, _properties);
            }
            else
            {
                transaction.KeyOffType = KeyOffType.LocalHandpay;

                using (var unitOfWork = _unitOfWorkFactory.Create())
                {
                    var voucher = unitOfWork.Repository<Voucher>().Queryable().SingleOrDefault();
                    if (voucher != null)
                    {
                        unitOfWork.BeginTransaction(IsolationLevel.ReadCommitted);

                        transaction.TicketData = voucher.VoucherTicketData();

                        unitOfWork.Repository<Voucher>().Delete(voucher);
                        unitOfWork.Commit();
                    }
                }
            }

            await Task.CompletedTask;
        }

        private void SetTicketData(
            HandpayTransaction transaction,
            IUnitOfWorkFactory unitOfWorkFactory,
            IPropertiesManager properties)
        {
            using (var unitOfWork = unitOfWorkFactory.Create())
            {
                var instance = unitOfWork.Repository<Instance>().Queryable().FirstOrDefault();

                var ticketData = new Dictionary<string, string>();
                transaction.TicketData = ticketData;

                ticketData[TicketType] = HandpayReceiptNoBarcode;
                ticketData[EstablishmentName] = properties.GetValue(AttributeNames.LocationName, DataUnavailable);
                ticketData[LocationName] = properties.GetValue(AttributeNames.LocationAddress, DataUnavailable);
                ticketData[Title] = HandpayTitle;
                ticketData[TicketNumber2] = $"{instance?.DeviceId ?? 0} {0} {instance?.SiteId}";
                ticketData[Datetime] = DateTime.Now.ToString("MMM dd yyyy");
                ticketData[License] = DateTime.Now.ToString("hh:mmtt");

                var totalAmount = transaction.CashableAmount + transaction.PromoAmount;
                ticketData[Value] = totalAmount.MillicentsToDollars().FormattedCurrencyString();
                ticketData[ValueInWords1] =
                    CurrencyExtensions.ConvertCurrencyToWords(totalAmount.MillicentsToDollars());

                if (transaction.PromoAmount <= 0)
                {
                    return;
                }

                ticketData[ExpiryDate2] =
                    $"Cash: {transaction.CashableAmount.MillicentsToDollars().FormattedCurrencyString()}";
                ticketData[Asset] =
                    $"{Coupon}: {transaction.PromoAmount.MillicentsToDollars().FormattedCurrencyString()}";
            }
        }*/
    }
}
