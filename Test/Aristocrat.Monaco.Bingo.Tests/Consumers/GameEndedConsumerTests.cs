namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Bingo.Consumers;
    using Commands;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameEndedConsumerTests
    {
        private const string MachineId = "TestMachine";
        private const long HistoryId = 1;

        private readonly Mock<ICentralProvider> _provider = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new(MockBehavior.Default);
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _factory = new(MockBehavior.Default);
        private readonly Mock<IGameHistoryLog> _historyLog = new(MockBehavior.Default);

        private GameEndedEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _factory.Setup(x => x.Execute(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _properties.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(MachineId);
            _historyLog.Setup(x => x.TransactionId).Returns(HistoryId);
            _historyLog.Setup(x => x.ShallowCopy()).Returns(_historyLog.Object);
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
        }

        [TestMethod]
        public async Task ConsumeNoTransactionsTest()
        {
            _provider.Setup(x => x.Transactions).Returns(Enumerable.Empty<CentralTransaction>());
            var endEvent = new GameEndedEvent(123, 1000, string.Empty, _historyLog.Object);
            await _target.Consume(endEvent, CancellationToken.None);
            _factory.Verify(x => x.Execute(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task ConsumerWithTransactionTest()
        {
            const long centralId = 4;
            var transaction = new CentralTransaction
            {
                AssociatedTransactions = new List<long> { HistoryId },
                TransactionId = centralId
            };

            var transactions = new List<CentralTransaction> { transaction };

            _provider.Setup(x => x.Transactions).Returns(transactions);
            var endEvent = new GameEndedEvent(123, 1000, string.Empty, _historyLog.Object);
            await _target.Consume(endEvent, CancellationToken.None);
            _factory.Verify(
                x => x.Execute(
                    It.Is<BingoGameEndedCommand>(
                        c => c.Transaction == transaction && c.MachineSerial == MachineId &&
                             c.Log == _historyLog.Object),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [DataRow(true, false, false, false)]
        [DataRow(false, true, false, false)]
        [DataRow(false, false, true, false)]
        [DataRow(false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(
            bool nullFactory,
            bool providerNull,
            bool nullProperties,
            bool eventBusNull)
        {
            _ = CreateTarget(nullFactory, providerNull, nullProperties, eventBusNull);
        }

        private GameEndedEventConsumer CreateTarget(
            bool nullFactory = false,
            bool providerNull = false,
            bool nullProperties = false,
            bool eventBusNull = false)
        {
            return new GameEndedEventConsumer(
                nullFactory ? null : _factory.Object,
                providerNull ? null : _provider.Object,
                nullProperties ? null : _properties.Object,
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object);
        }
    }
}