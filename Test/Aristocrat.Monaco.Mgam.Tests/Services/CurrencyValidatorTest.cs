namespace Aristocrat.Monaco.Mgam.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Aristocrat.Monaco.Mgam.Common.Events;
    using Aristocrat.Monaco.Kernel;
    using Aristocrat.Monaco.Protocol.Common.Storage.Entity;
    using Commands;
    using Common.Data.Models;
    using Mgam.Services.CreditValidators;
    using Mgam.Services.Lockup;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;
    using BeginSession = Commands.BeginSession;
    using CreditCash = Commands.CreditCash;
    using EscrowCash = Commands.EscrowCash;
    using Aristocrat.Monaco.Gaming.Contracts;

    [TestClass]
    public class CurrencyValidatorTest
    {
        private Mock<ILogger<CurrencyValidator>> _logger;
        private Mock<ICommandHandlerFactory> _commandFactory;
        private Mock<IUnitOfWorkFactory> _unitOfWorkFactory;
        private Mock<ILockup> _lockoutHandler;
        private Mock<IEventBus> _eventBus;
        private Mock<IPlayerBank> _bank;

        private CurrencyValidator _target;

        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Strict);

            _logger = new Mock<ILogger<CurrencyValidator>>();
            _commandFactory = new Mock<ICommandHandlerFactory>();
            _unitOfWorkFactory = new Mock<IUnitOfWorkFactory>();
            _lockoutHandler = new Mock<ILockup>();
            _eventBus = new Mock<IEventBus>();
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<InstanceRegisteredEvent>>()));
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<HostOnlineEvent>>()));
            _bank = new Mock<IPlayerBank>();
            _bank.SetupGet(x => x.Balance).Returns(1000);
            _target = new CurrencyValidator(
                _logger.Object,          
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _lockoutHandler.Object,
                _eventBus.Object,
                _bank.Object);
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
            var service = new CurrencyValidator(
                null,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandFactoryException()
        {
            var service = new CurrencyValidator(
                _logger.Object,
                null,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullUnitOfWorkFactoryExpectException()
        {
            var service = new CurrencyValidator(
                _logger.Object,
                _commandFactory.Object,
                null,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullLockoutExpectException()
        {
            var service = new CurrencyValidator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                null,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEventBusExpectException()
        {
            var service = new CurrencyValidator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _lockoutHandler.Object,
                null,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullBankExpectException()
        {
            var service = new CurrencyValidator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _lockoutHandler.Object,
                _eventBus.Object,
                null);

            Assert.IsNull(service);
        }

        [TestMethod]
        public void WhenConstructExpectSuccess()
        {
            var service = new CurrencyValidator(
                _logger.Object,
                _commandFactory.Object,
                _unitOfWorkFactory.Object,
                _lockoutHandler.Object,
                _eventBus.Object,
                _bank.Object);

            Assert.IsNotNull(service);
            Assert.IsInstanceOfType(service, typeof(ICurrencyValidator));
        }

        [TestMethod]
        public void ValidateNoteSuccess()
        {
            _commandFactory.Setup(c => c.Execute(It.IsAny<EscrowCash>())).Returns(Task.CompletedTask).Verifiable();
            var test = DataModelHelpers.SetUpDataModel(_unitOfWorkFactory, new Session { SessionId = 1 });

            var result = _target.ValidateNote(1).Result;

            _commandFactory.Verify();
            test.repo.Verify();

            Assert.AreEqual(CurrencyInExceptionCode.None, result);
        }

        [TestMethod]
        public void ValidateNoteEscrowCashFailedCreditFailedSessionBalanceLimit()
        {
            _commandFactory.Setup(c => c.Execute(It.IsAny<EscrowCash>())).Throws(
                new ServerResponseException(ServerResponseCode.CreditFailedSessionBalanceLimit)).Verifiable();

            var result = _target.ValidateNote(1).Result;

            _commandFactory.Verify();

            Assert.AreEqual(CurrencyInExceptionCode.CreditInLimitExceeded, result);
        }

        [TestMethod]
        public void ValidateNoteEscrowCashFailedCreditFailedInvalidAmount()
        {
            _commandFactory.Setup(c => c.Execute(It.IsAny<EscrowCash>()))
                .Throws(new ServerResponseException(ServerResponseCode.InvalidAmount)).Verifiable();

            var result = _target.ValidateNote(1).Result;

            _commandFactory.Verify();

            Assert.AreEqual(CurrencyInExceptionCode.InvalidBill, result);
        }

        [TestMethod]
        public void ValidateNoteEscrowCashFailedCreditFailedServerError()
        {
            _commandFactory.Setup(c => c.Execute(It.IsAny<EscrowCash>()))
                .Throws(new ServerResponseException(ServerResponseCode.ServerError)).Verifiable();

            var result = _target.ValidateNote(1).Result;

            _commandFactory.Verify();

            Assert.AreEqual(CurrencyInExceptionCode.Other, result);
        }

        [TestMethod]
        public void ValidateNoteSessionFailed()
        {
            _commandFactory.Setup(c => c.Execute(It.IsAny<EscrowCash>())).Returns(Task.CompletedTask).Verifiable();
            _commandFactory.Setup(c => c.Execute(It.IsAny<BeginSession>()))
                .Throws(new ServerResponseException(ServerResponseCode.ServerError)).Verifiable();
            var test = DataModelHelpers.SetUpDataModel(_unitOfWorkFactory, default(Session));

            var result = _target.ValidateNote(1).Result;

            _commandFactory.Verify();
            test.repo.Verify();

            Assert.AreEqual(CurrencyInExceptionCode.Other, result);
        }

        [TestMethod]
        public void StackedNoteSuccess()
        {
            var test = DataModelHelpers.SetUpDataModel(_unitOfWorkFactory, new Session { SessionId = 1 });
            _commandFactory.Setup(c => c.Execute(It.IsAny<CreditCash>())).Returns(Task.CompletedTask).Verifiable();

            var result = _target.StackedNote(1).Result;

            _commandFactory.Verify();

            Assert.IsTrue(result);
            test.repo.Verify();
        }
    }
}