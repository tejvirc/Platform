namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts.Handpay;
    using Aristocrat.Mgam.Client;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Monaco.Mgam.Services.Notification;
    using Commands;
    using Gaming.Contracts;
    using Mgam.Consumers;
    using Mgam.Services.Lockup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using EndSession = Commands.EndSession;

    [TestClass]
    public class HandpayCompletedConsumerTest
    {
        private Mock<ILockup> _lockup;
        private Mock<IPlayerBank> _bank;
        private Mock<ILogger<HandpayCompletedConsumer>> _logger;
        private Mock<ICommandHandlerFactory> _commandFactory;
        private Mock<INotificationLift> _notificationLift;
        private HandpayCompletedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _lockup = new Mock<ILockup>(MockBehavior.Default);
            _bank = new Mock<IPlayerBank>();
            _logger = new Mock<ILogger<HandpayCompletedConsumer>>(MockBehavior.Default);
            _commandFactory = new Mock<ICommandHandlerFactory>(MockBehavior.Default);
            _notificationLift = new Mock<INotificationLift>(MockBehavior.Default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LockupNullLockupHandlerTest()
        {
            _target = new HandpayCompletedConsumer(
                null,
                _notificationLift.Object,
                _commandFactory.Object,
                _logger.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmployeeLoginNullNotificationTest()
        {
            _target = new HandpayCompletedConsumer(
                _lockup.Object,
                null,
                _commandFactory.Object,
                _logger.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LoggerNullCommandFactoryTest()
        {
            _target = new HandpayCompletedConsumer(
                _lockup.Object,
                _notificationLift.Object,
                null,
                _logger.Object,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CommandFactoryNullLoggerTest()
        {
            _target = new HandpayCompletedConsumer(
                _lockup.Object,
                _notificationLift.Object,
                _commandFactory.Object,
                null,
                _bank.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NotificationLiftNullBankTest()
        {
            _target = new HandpayCompletedConsumer(
                _lockup.Object,
                _notificationLift.Object,
                _commandFactory.Object,
                _logger.Object,
                null);
        }

        [TestMethod]
        public void SuccessfulConstructor()
        {
            _target = new HandpayCompletedConsumer(
                _lockup.Object,
                _notificationLift.Object,
                _commandFactory.Object,
                _logger.Object,
                _bank.Object);

            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void SuccessConsumerCancelCredit()
        {
            _target = new HandpayCompletedConsumer(
                _lockup.Object,
                _notificationLift.Object,
                _commandFactory.Object,
                _logger.Object,
                _bank.Object);

            _bank.SetupGet(b => b.Balance).Returns(0).Verifiable();

            _notificationLift.Setup(n => n.Notify(NotificationCode.CanceledCreditHandPay, null))
                .Returns(Task.CompletedTask).Verifiable();

            _commandFactory.Setup(c => c.Execute(It.IsAny<EndSession>())).Returns(Task.CompletedTask).Verifiable();

            _target.Consume(
                new HandpayCompletedEvent(
                    new HandpayTransaction(1, DateTime.Today, 1000, 0, 0, HandpayType.CancelCredit, false, Guid.Empty)),
                CancellationToken.None).Wait(10);

            _notificationLift.Verify();
            _bank.Verify();
            _commandFactory.Verify();
        }

        [TestMethod]
        public void FailConsumerCancelCredit()
        {
            _target = new HandpayCompletedConsumer(
                _lockup.Object,
                _notificationLift.Object,
                _commandFactory.Object,
                _logger.Object,
                _bank.Object);

            _bank.SetupGet(b => b.Balance).Returns(0).Verifiable();

            _notificationLift.Setup(n => n.Notify(NotificationCode.CanceledCreditHandPay, null))
                .Returns(Task.CompletedTask).Verifiable();

            _commandFactory.Setup(c => c.Execute(It.IsAny<EndSession>()))
                .Throws(new ServerResponseException(ServerResponseCode.ServerError)).Verifiable();

            _target.Consume(
                new HandpayCompletedEvent(
                    new HandpayTransaction(1, DateTime.Today, 1000, 0, 0, HandpayType.CancelCredit, false, Guid.Empty)),
                CancellationToken.None).Wait(10);

            _notificationLift.Verify();
            _bank.Verify();
            _commandFactory.Verify();
        }

        [TestMethod]
        public void SuccessConsumerGameWin()
        {
            _target = new HandpayCompletedConsumer(
                _lockup.Object,
                _notificationLift.Object,
                _commandFactory.Object,
                _logger.Object,
                _bank.Object);

            _notificationLift.Setup(n => n.Notify(It.IsAny<NotificationCode>(), It.IsAny<string>())).Returns(Task.CompletedTask)
                .Verifiable();

            _target.Consume(
                new HandpayCompletedEvent(
                    new HandpayTransaction(1, DateTime.Today, 1000, 0, 0, HandpayType.CancelCredit, false, Guid.Empty)),
                CancellationToken.None).Wait(10);

            _notificationLift.Verify();
        }
    }
}