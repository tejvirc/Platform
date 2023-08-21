namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Accounting.Contracts;
    using Consumers;
    using Contracts;
    using Contracts.Barkeeper;
    using Gaming.Commands;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CurrencyInConsumerTests
    {
        private CurrencyInConsumer _target;
        private Mock<ICommandHandlerFactory> _commandHandlerFactory;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<IPlayerBank> _playerBank;
        private Mock<ICommandHandler<CheckBalance>> _commandHandler;
        private Mock<IEventBus> _eventBus;
        private Mock<IBarkeeperHandler> _barkeeperHandler;
        private Mock<ICurrencyInContainer> _currencyIn;
        private Mock<ISessionInfoService> _session;
        private Mock<IPersistentStorageManager> _persistence;
        private Mock<IScopedTransaction> _scope;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _commandHandlerFactory = new Mock<ICommandHandlerFactory>(MockBehavior.Default);
            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _playerBank = new Mock<IPlayerBank>(MockBehavior.Default);
            _commandHandler = new Mock<ICommandHandler<CheckBalance>>(MockBehavior.Default);
            _barkeeperHandler = new Mock<IBarkeeperHandler>(MockBehavior.Default);
            _currencyIn = new Mock<ICurrencyInContainer>(MockBehavior.Default);
            _session = new Mock<ISessionInfoService>(MockBehavior.Default);
            _persistence = new Mock<IPersistentStorageManager>(MockBehavior.Default);
            _scope = new Mock<IScopedTransaction>(MockBehavior.Default);
            _persistence.Setup(x => x.ScopedTransaction()).Returns(_scope.Object);

            _target = CreateConsumer();
        }

        private CurrencyInConsumer CreateConsumer(
            bool nullEventBus = false,
            bool nullCommandHandler = false,
            bool nullProperties = false,
            bool nullPlayerBank = false,
            bool nullBarkeeper = false,
            bool nullCurrencyIn = false,
            bool nullSession = false,
            bool nullPersistence = false)
        {
            return new CurrencyInConsumer(
                nullEventBus ? null : _eventBus.Object,
                nullCommandHandler ? null : _commandHandlerFactory.Object,
                nullProperties ? null : _propertiesManager.Object,
                nullPlayerBank ? null : _playerBank.Object,
                nullBarkeeper ? null : _barkeeperHandler.Object,
                nullCurrencyIn ? null : _currencyIn.Object,
                nullSession ? null : _session.Object,
                nullPersistence ? null : _persistence.Object);
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false, false, false)]
        [DataRow(false, false, true, false, false, false, false, false)]
        [DataRow(false, false, false, true, false, false, false, false)]
        [DataRow(false, false, false, false, true, false, false, false)]
        [DataRow(false, false, false, false, false, true, false, false)]
        [DataRow(false, false, false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullEventBus,
            bool nullCommandHandler,
            bool nullProperties,
            bool nullPlayerBank,
            bool nullBarkeeper,
            bool nullCurrencyIn,
            bool nullSession,
            bool nullPersistence)
        {
            CreateConsumer(
                nullEventBus,
                nullCommandHandler,
                nullProperties,
                nullPlayerBank,
                nullBarkeeper,
                nullCurrencyIn,
                nullSession,
                nullPersistence);
        }

        [DataRow(false, 1000, 2000, false, DisplayName = "Not Allowing Credits Above Max will not check balance")]
        [DataRow(true, 1000, 2000, true, DisplayName = "Allowing Credits Above Max will check balance if balance is exceeded")]
        [DataRow(true, 3000, 2000, false, DisplayName = "Allowing Credits Above Max will check not balance if balance is not exceeded")]
        [DataTestMethod]
        public void ConsumeWithValidAmountTest(
            bool allowCreditsInAboveMaxCredits,
            long maxCreditMeter,
            long currentBalance,
            bool handleCheckBalance)
        {
            const long expectedAmount = 10000;
            _propertiesManager.Setup(x => x.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(maxCreditMeter);
            _propertiesManager
                .Setup(x => x.GetProperty(AccountingConstants.AllowCreditsInAboveMaxCredit, It.IsAny<bool>()))
                .Returns(allowCreditsInAboveMaxCredits);
            _playerBank.Setup(x => x.Balance).Returns(currentBalance);

            if (handleCheckBalance)
            {
                _commandHandlerFactory.Setup(x => x.Create<CheckBalance>()).Returns(_commandHandler.Object);
                _commandHandler.Setup(x => x.Handle(It.IsAny<CheckBalance>())).Verifiable();
            }

            var transaction = new BillTransaction();
            _target.Consume(new CurrencyInCompletedEvent(expectedAmount, null, transaction));
            _commandHandler.Verify();
            _barkeeperHandler.Verify(x => x.OnCreditsInserted(expectedAmount));
            _session.Verify(x => x.HandleTransaction(transaction));
            _currencyIn.Verify(x => x.Credit(transaction));
        }
    }
}