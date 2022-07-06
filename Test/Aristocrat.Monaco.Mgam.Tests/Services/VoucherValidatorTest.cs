namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Accounting.Contracts.TransferOut;
    using Accounting.Contracts.Vouchers;
    using Application.Contracts.Extensions;
    using Application.Contracts.Localization;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Mgam.Common;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Aristocrat.Monaco.Protocol.Common.Storage.Repositories;
    using Commands;
    using Common.Data.Models;
    using Common.Events;
    using Gaming.Contracts;
    using Hardware.Contracts.Printer;
    using Hardware.Contracts.SharedDevice;
    using Kernel;
    using Mgam.Services.Attributes;
    using Mgam.Services.CreditValidators;
    using Mgam.Services.Lockup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using BeginSession = Commands.BeginSession;
    using CreditVoucher = Commands.CreditVoucher;
    using ValidateVoucher = Commands.ValidateVoucher;

    [TestClass]
    public class VoucherValidatorTest
    {
        private const int WaitTime = 1000;

        private Mock<IEventBus> _eventBus;
        private Mock<ILogger<VoucherValidator>> _logger;
        private Mock<INotificationLift> _notifier;
        private Mock<IPropertiesManager> _properties;
        private Mock<IAttributeManager> _attributes;
        private Mock<ICashOut> _cashOut;
        private Mock<IEgm> _egm;
        private Mock<ILockup> _lock;
        private Mock<ICommandHandlerFactory> _commandFactory;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
        private Mock<ITransactionHistory> _transactionHistory;
        private Mock<IGameHistory> _gameHistory;
        private Mock<ITransactionRetryHandler> _retryHandler;
        private Mock<IIdProvider> _idProvider;

        private IVoucherValidator _target;

        private Action<VoucherIssuedEvent> _subscriptionToVoucherIssuedEvent;
        private Action<TransferOutFailedEvent> _subscriptionToTransferOutFailedEvent;
        private Action<LockupResolvedEvent> _subscriptionToLockupResolvedEvent;
        private Action<EnabledEvent> _subscriptionToEnabledEvent;
        private Action<ReadyToPlayEvent> _subscriptionToReadyToPlayEvent;
        private Action<HostOfflineEvent> _subscriptionToHostOfflineEvent;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);
            var factory = new Mock<ILocalizerFactory>();
            MoqServiceManager.Instance.Setup(m => m.GetService<ILocalizerFactory>()).Returns(factory.Object);
            factory.Setup(x => x.For(It.IsAny<string>())).Returns(new Mock<ILocalizer>().Object);

            RegionInfo region = new RegionInfo(CultureInfo.CurrentCulture.Name);
            CurrencyExtensions.SetCultureInfo(region.ISOCurrencySymbol, CultureInfo.CurrentCulture, null, null, true, true, "c");

            _logger = new Mock<ILogger<VoucherValidator>>();

            _eventBus = new Mock<IEventBus>();

            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<VoucherIssuedEvent>>()))
                .Callback<object, Action<VoucherIssuedEvent>>(
                    (_, callback) => _subscriptionToVoucherIssuedEvent = callback);
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<TransferOutFailedEvent>>()))
                .Callback<object, Action<TransferOutFailedEvent>>(
                    (_, callback) => _subscriptionToTransferOutFailedEvent = callback);
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<LockupResolvedEvent>>()))
                .Callback<object, Action<LockupResolvedEvent>>(
                    (_, callback) => _subscriptionToLockupResolvedEvent = callback);
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<EnabledEvent>>()))
                .Callback<object, Action<EnabledEvent>>((_, callback) => _subscriptionToEnabledEvent = callback);
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<ReadyToPlayEvent>>()))
                .Callback<object, Action<ReadyToPlayEvent>>((_, callback) => _subscriptionToReadyToPlayEvent = callback);
            _eventBus.Setup(b => b.Subscribe(It.IsAny<object>(), It.IsAny<Action<HostOfflineEvent>>()))
                .Callback<object, Action<HostOfflineEvent>
                >((_, callback) => _subscriptionToHostOfflineEvent = callback);

            _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            _commandFactory = new Mock<ICommandHandlerFactory>();
            _lock = new Mock<ILockup>();
            _egm = new Mock<IEgm>();
            _properties = new Mock<IPropertiesManager>();
            _properties.Setup(p => p.GetProperty(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty)
                .Verifiable();
            _attributes = new Mock<IAttributeManager>();
            _attributes.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(string.Empty)
                .Verifiable();
            _attributes.Setup(p => p.Get(It.IsAny<string>(), It.IsAny<int>())).Returns(MgamConstants.DefaultSessionBalanceLimit)
                .Verifiable();
            _cashOut = new Mock<ICashOut>();
            _transactionHistory = new Mock<ITransactionHistory>();
            _transactionHistory.Setup(t => t.UpdateTransaction(It.IsAny<VoucherInTransaction>())).Verifiable();
            _notifier = new Mock<INotificationLift>();
            _gameHistory = new Mock<IGameHistory>();
            _gameHistory.SetupGet(g => g.IsRecoveryNeeded).Returns(false);

            _retryHandler = new Mock<ITransactionRetryHandler>();
            _idProvider = new Mock<IIdProvider>();

            CreateNewTarget();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(false, true, true, true, true, true, true, true, true, true, true, true, true, true, DisplayName = "Null Logger Object")]
        [DataRow(true, false, true, true, true, true, true, true, true, true, true, true, true, true, DisplayName = "Null Event Bus Object")]
        [DataRow(true, true, false, true, true, true, true, true, true, true, true, true, true, true, DisplayName = "Null Properties Manager Object")]
        [DataRow(true, true, true, false, true, true, true, true, true, true, true, true, true, true, DisplayName = "Null Attribute Manager Object")]
        [DataRow(true, true, true, true, false, true, true, true, true, true, true, true, true, true, DisplayName = "Null Bank Object")]
        [DataRow(true, true, true, true, true, false, true, true, true, true, true, true, true, true, DisplayName = "Null Command Factory Object")]
        [DataRow(true, true, true, true, true, true, false, true, true, true, true, true, true, true, DisplayName = "Null EGM Object")]
        [DataRow(true, true, true, true, true, true, true, false, true, true, true, true, true, true, DisplayName = "Null Unit of Work Factory Object")]
        [DataRow(true, true, true, true, true, true, true, true, false, true, true, true, true, true, DisplayName = "Null Lockup Object")]
        [DataRow(true, true, true, true, true, true, true, true, true, false, true, true, true, true, DisplayName = "Null Notifier Object")]
        [DataRow(true, true, true, true, true, true, true, true, true, true, false, true, true, true, DisplayName = "Null Transaction History Object")]
        [DataRow(true, true, true, true, true, true, true, true, true, true, true, false, true, true, DisplayName = "Null Game History Object")]
        [DataRow(true, true, true, true, true, true, true, true, true, true, true, true, false, true, DisplayName = "Null Retry Handler Object")]
        [DataRow(true, true, true, true, true, true, true, true, true, true, true, true, true, false, DisplayName = "Null ID Provider Object")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorNullParameterTest(
            bool logger,
            bool eventBus,
            bool properties,
            bool attributeManager,
            bool cashOut,
            bool commandFactory,
            bool egm,
            bool unitOfWork,
            bool lockup,
            bool notifier,
            bool transactionHistory,
            bool gameHistory,
            bool retryHandler,
            bool idProvider)
        {
            _target = new VoucherValidator(
                logger ? _logger.Object : null,
                eventBus ? _eventBus.Object : null,
                properties ? _properties.Object : null,
                attributeManager ? _attributes.Object : null,
                cashOut ? _cashOut.Object : null,
                commandFactory ? _commandFactory.Object : null,
                egm ? _egm.Object : null,
                unitOfWork ? _unitOfWorkFactory.Object : null,
                lockup ? _lock.Object : null,
                notifier ? _notifier.Object : null,
                transactionHistory ? _transactionHistory.Object : null,
                gameHistory ? _gameHistory.Object : null,
                retryHandler ? _retryHandler.Object : null,
                idProvider ? _idProvider.Object : null);
        }

        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            Assert.IsNotNull(_target);
            Assert.IsInstanceOfType(_target, typeof(IVoucherValidator));
        }

        [TestMethod]
        public void RedeemVoucherTest()
        {
            DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Session { Id = 1, SessionId = 1, OfflineVoucherBarcode = "1234", OfflineVoucherPrinted = false });

            _commandFactory.Setup(c => c.Execute(It.IsAny<ValidateVoucher>())).Returns(Task.CompletedTask)
                .Verifiable();

            _cashOut.SetupGet(s => s.Balance).Returns(1000);

            var voucherIn = new VoucherInTransaction(1, DateTime.Today, 10000, AccountType.Cashable, "1234");

            var amount = _target.RedeemVoucher(voucherIn).Result;

            _commandFactory.Verify();

            Assert.AreEqual(voucherIn.Amount, amount.Amount);
        }

        [TestMethod]
        public void RedeemVoucherFailedTest()
        {
            _commandFactory.Setup(c => c.Execute(It.IsAny<ValidateVoucher>()))
                .Throws(new ServerResponseException(ServerResponseCode.ServerError))
                .Verifiable();

            var voucherIn = new VoucherInTransaction(1, DateTime.Today, 10000, AccountType.Cashable, "1234");

            var amount = _target.RedeemVoucher(voucherIn).Result;

            _commandFactory.Verify();

            Assert.AreEqual(voucherIn.Exception, (int)VoucherInExceptionCode.Other);
            Assert.IsNull(amount);
        }

        [TestMethod]
        public void RedeemVoucherFailedSessionCommandTest()
        {
            DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                default(Session));

            _cashOut.SetupGet(s => s.Balance).Returns(1000);

            _commandFactory.Setup(c => c.Execute(It.IsAny<ValidateVoucher>())).Returns(Task.CompletedTask)
                .Verifiable();

            _commandFactory.Setup(c => c.Execute(It.IsAny<BeginSession>()))
                .Throws(new ServerResponseException(ServerResponseCode.ServerError))
                .Verifiable();

            _lock.Setup(l => l.LockupForEmployeeCard(It.IsAny<string>(), SystemDisablePriority.Immediate)).Verifiable();

            var voucherIn = new VoucherInTransaction(1, DateTime.Today, 10000, AccountType.Cashable, "1234");

            var amount = _target.RedeemVoucher(voucherIn).Result;

            _commandFactory.Verify();
            _lock.Verify();

            Assert.AreEqual(voucherIn.Exception, (int)VoucherInExceptionCode.Other);
            Assert.IsNull(amount);
        }

        [TestMethod]
        public void CanValidateVoucherTest()
        {
            const long amount = 100;
            Assert.IsTrue(_target.CanValidateVoucherOut(amount, AccountType.Cashable));
        }

        [TestMethod]
        public void IssueVoucherTest()
        {
            var voucher = new Voucher();
            voucher.Validate();
            voucher.VoucherBarcode = "12345678";

            var setUp = DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                voucher);

            setUp.repo.Setup(r => r.Delete(It.IsAny<Voucher>())).Verifiable();

            DataModelHelpers.AddRepoNoValue<Session>(setUp.unit);

            _subscriptionToVoucherIssuedEvent(new VoucherIssuedEvent(null, null));

            setUp.repo.Verify();
            _cashOut.SetupGet(s => s.Balance).Returns(1000);

            _subscriptionToHostOfflineEvent(new HostOfflineEvent());

            Assert.IsFalse(_target.HostOnline);

            var result = _target.IssueVoucher(
                new VoucherAmount(10000, 10000, 0),
                AccountType.Cashable,
                Guid.NewGuid(),
                TransferOutReason.CashOut).Result;

            setUp.repo.Verify();

            Assert.AreEqual(20000, result.Amount);
            Assert.IsNotNull(result.TicketData);
        }

        [TestMethod]
        public void IssueVoucherOfflineTest()
        {
            var setUp = DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Voucher());

            setUp.repo.Setup(r => r.Delete(It.IsAny<Voucher>())).Verifiable();

            _subscriptionToVoucherIssuedEvent(new VoucherIssuedEvent(null, null));

            setUp.repo.Verify();
            _cashOut.SetupGet(s => s.Balance).Returns(1000);

            _subscriptionToHostOfflineEvent(new HostOfflineEvent());

            Assert.IsFalse(_target.HostOnline);

            var sessionRepo = new Mock<IRepository<Session>>();
            var session = new Session
            {
                OfflineVoucherBarcode = "1234",
                Id = 1,
                SessionId = 1,
                OfflineVoucherPrinted = false
            };
            var sessionQuery = new DataModelHelpers.MockDbSet<Session>(new List<Session> { session });
            sessionRepo.Setup(r => r.Queryable()).Returns(sessionQuery);

            sessionRepo.Setup(r => r.Update(It.IsAny<Session>())).Verifiable();
            setUp.unit.Setup(u => u.Repository<Session>()).Returns(sessionRepo.Object);
            setUp.repo.Setup(r => r.Add(It.IsAny<Voucher>())).Verifiable();

            var result = _target.IssueVoucher(
                new VoucherAmount(10000, 10000, 0),
                AccountType.Cashable,
                Guid.NewGuid(),
                TransferOutReason.CashOut).Result;

            setUp.repo.Verify();
            sessionRepo.Verify();

            Assert.IsTrue(session.OfflineVoucherPrinted);
            Assert.AreEqual(20000, result.Amount);
            Assert.IsNotNull(result.TicketData);
        }

        [TestMethod]
        public void StackedVoucherTest()
        {
            _commandFactory.Setup(c => c.Execute(It.IsAny<CreditVoucher>())).Returns(Task.CompletedTask).Verifiable();
            var transaction = new VoucherInTransaction(1, DateTime.Today, "1234") { State = VoucherState.Redeemed };

            _target.StackedVoucher(transaction).Wait(WaitTime);
            _commandFactory.Verify();
        }

        [TestMethod]
        public void VoucherRedeemedTest()
        {
            var transaction = new VoucherInTransaction { State = VoucherState.Redeemed };
            _target.CommitVoucher(transaction);
            _transactionHistory.Verify();
            Assert.AreEqual(true, transaction.CommitAcknowledged);
        }

        [TestMethod]
        public void VoucherRejectedTest()
        {
            var transaction = new VoucherInTransaction { State = VoucherState.Rejected };
            _target.CommitVoucher(transaction);
            _transactionHistory.Verify();
            Assert.AreEqual(true, transaction.CommitAcknowledged);
        }

        [TestMethod]
        public void HandleHostOfflineEvent()
        {
            _subscriptionToHostOfflineEvent(new HostOfflineEvent());

            Assert.IsFalse(_target.HostOnline);
        }

        [TestMethod]
        public void HandleReadyToPlayTest()
        {
            var voucher = new Voucher();
            voucher.Validate();
            voucher.OfflineReason = VoucherOutOfflineReason.Credit;
            var setUp = DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                voucher);
            setUp.repo.Setup(r => r.Update(It.IsAny<Voucher>())).Verifiable();

            _subscriptionToReadyToPlayEvent(new ReadyToPlayEvent());

            Assert.IsTrue(voucher.OfflineReason == VoucherOutOfflineReason.None);
            Assert.IsTrue(_target.HostOnline);
            setUp.repo.Verify();
        }

        [TestMethod]
        public void HandleEnabledEventTest()
        {
            DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Voucher());

            _notifier.Setup(n => n.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>())).Verifiable();

            _subscriptionToTransferOutFailedEvent(new TransferOutFailedEvent(0, 0, 0, Guid.Empty));

            _lock.SetupGet(l => l.IsLockedForEmployeeCard).Returns(false).Verifiable();
            _cashOut.Setup(b => b.CashOut()).Verifiable();

            _subscriptionToEnabledEvent(new EnabledEvent(EnabledReasons.GamePlay));

            _lock.Verify();
            _notifier.Verify();
            _cashOut.Verify();
        }

        [TestMethod]
        public void HandleLockupResolvedEventTest()
        {
            DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Voucher());

            _notifier.Setup(n => n.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>())).Verifiable();

            _subscriptionToTransferOutFailedEvent(new TransferOutFailedEvent(0, 0, 0, Guid.Empty));

            _lock.SetupGet(l => l.IsLockedForEmployeeCard).Returns(false).Verifiable();
            _cashOut.Setup(b => b.CashOut()).Verifiable();

            _subscriptionToLockupResolvedEvent(new LockupResolvedEvent());

            _lock.Verify();
            _notifier.Verify();
            _cashOut.Verify();
        }

        [TestMethod]
        public void HandleTransferOutFailedEventTest()
        {
            DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Voucher());
            _notifier.Setup(n => n.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>())).Verifiable();

            _subscriptionToTransferOutFailedEvent(new TransferOutFailedEvent(0, 0, 0, Guid.Empty));

            _notifier.Verify();
        }

        [TestMethod]
        public void HandleVoucherIssuedEventTest()
        {
            var setUp = DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Voucher());
            setUp.repo.Setup(r => r.Delete(It.IsAny<Voucher>())).Verifiable();

            _subscriptionToVoucherIssuedEvent(new VoucherIssuedEvent(null, null));

            setUp.repo.Verify();
        }

        private void CreateNewTarget()
        {
            var setUp = DataModelHelpers.SetUpDataModel(
                _unitOfWorkFactory,
                new Voucher());
            
            _target = new VoucherValidator(
                _logger.Object,
                _eventBus.Object,
                _properties.Object,
                _attributes.Object,
                _cashOut.Object,
                _commandFactory.Object,
                _egm.Object,
                _unitOfWorkFactory.Object,
                _lock.Object,
                _notifier.Object,
                _transactionHistory.Object,
                _gameHistory.Object,
                _retryHandler.Object,
                _idProvider.Object);

            _target.Initialize();
        }
    }
}