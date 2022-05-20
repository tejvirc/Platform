namespace Aristocrat.Monaco.Hhr.Tests.Services
{
    using System;
    using Accounting.Contracts;
    using Events;
    using Hhr.Commands;
    using Hhr.Services.GamePlay;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using Storage.Helpers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CentralHandlerTests
    {
        private readonly CentralHandler _target;
        private Mock<IBank> _bank;
        private Mock<ICentralProvider> _centralProvider;
        private Mock<ICommandHandlerFactory> _commandFactory;
        private Action<GameEndedEvent> _eventAction;
        private Mock<IEventBus> _eventBus;
        private Mock<IGameHistoryLog> _gameLog;
        private Mock<IGameProvider> _gameProvider;
        private bool _prizeCalculationEventFired;
        private Mock<IGamePlayEntityHelper> _gamePlayEntityHelper;
        private Mock<ITransactionHistory> _transactionHistory;

        public CentralHandlerTests()
        {
            SetupMocks();
            SetupEventBus();
            _target = new CentralHandler(_centralProvider.Object, _commandFactory.Object, _bank.Object,
                _eventBus.Object, _gameProvider.Object, _gamePlayEntityHelper.Object, _transactionHistory.Object);
        }

        [DataRow(true, DisplayName = "Booting up with Prize Calculation Error")]
        [DataRow(false, DisplayName = "Booting up with No Prize Calculation Error")]
        [DataTestMethod]
        public void StartupWithPrizeCalculationError(bool isStartupError)
        {
            _prizeCalculationEventFired = false;
            _gamePlayEntityHelper.Setup(m => m.PrizeCalculationError).Returns(isStartupError);
            // ReSharper disable once UnusedVariable
            var centralLocal = new CentralHandler(_centralProvider.Object, _commandFactory.Object, _bank.Object,
                _eventBus.Object, _gameProvider.Object,_gamePlayEntityHelper.Object, _transactionHistory.Object);
            Assert.IsTrue(_prizeCalculationEventFired == isStartupError);
        }

        private void SetupMocks()
        {
            _centralProvider = new Mock<ICentralProvider>(MockBehavior.Default);
            _commandFactory = new Mock<ICommandHandlerFactory>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _gameLog = new Mock<IGameHistoryLog>(MockBehavior.Default);
            _gamePlayEntityHelper = new Mock<IGamePlayEntityHelper>(MockBehavior.Default);
            _transactionHistory = new Mock<ITransactionHistory>(MockBehavior.Default);
            _gameLog.Setup(g => g.ShallowCopy()).Returns(_gameLog.Object);
        }

        private void SetupEventBus()
        {
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OutcomeReceivedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(_target, It.IsAny<Action<OutcomeFailedEvent>>())).Verifiable();
            _eventBus.Setup(m => m.Subscribe(It.IsAny<ICentralHandler>(), It.IsAny<Action<GameEndedEvent>>()))
                .Callback<object, Action<GameEndedEvent>>((e, y) =>
                    _eventAction = y
                );
            _eventBus.Setup(m => m.Publish(It.IsAny<PrizeCalculationErrorEvent>()))
                .Callback<object>(e => _prizeCalculationEventFired = true);
        }
    }
}