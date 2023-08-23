namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts.Extensions;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Bingo.Consumers;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class CurrencyInCompletedConsumerTests
    {
        private const long TestAmount = 100;
        private CurrencyInCompletedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportTransactionQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _bingoEventQueue = new(MockBehavior.Strict);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new(MockBehavior.Default);
        private readonly CurrencyInCompletedEvent _event = new(TestAmount.CentsToMillicents(), new Note { Value = 1 });

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new CurrencyInCompletedConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _reportingService.Object,
                _bingoEventQueue.Object,
                _unitOfWorkFactory.Object,
                _gameProvider.Object);

            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(false);
        }

        [DataRow(true, false, false, false, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, false, false, false, DisplayName = "Reporting Transaction Service Null")]
        [DataRow(false, false, true, false, false, DisplayName = "Reporting Event Service Null")]
        [DataRow(false, false, false, true, false, DisplayName = "Unit of Work Factory Null")]
        [DataRow(false, false, false, false, true, DisplayName = "Properties manager Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(
            bool eventBusNull,
            bool reportingNull,
            bool queueNull,
            bool nullUnitOfWork,
            bool nullGameProvider)
        {
            _target = new CurrencyInCompletedConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                queueNull ? null : _bingoEventQueue.Object,
                nullUnitOfWork ? null : _unitOfWorkFactory.Object,
                nullGameProvider ? null : _gameProvider.Object);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashIn, TestAmount, 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashIn)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashIn, TestAmount, 0, 0, 0, 0, string.Empty),
                Times.Once());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashIn), Times.Once());
        }

        [TestMethod]
        public void ConsumesNullNoteTest()
        {
            var evt = new CurrencyInCompletedEvent(TestAmount.CentsToMillicents());
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashIn, TestAmount, 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashIn, TestAmount, 0, 0, 0, 0, string.Empty),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashIn), Times.Never());
        }

        [TestMethod]
        public void ConsumesZeroAmountTest()
        {
            var evt = new CurrencyInCompletedEvent(0, new Note());
            _reportingService.Setup(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashIn, 0, 0, 0, 0, 0, string.Empty)).Verifiable();
            _bingoEventQueue.Setup(m => m.AddNewEventToQueue(ReportableEvent.CashIn)).Verifiable();

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewTransactionToQueue(
                Common.TransactionType.CashIn, 0, 0, 0, 0, 0, string.Empty),
                Times.Never());
            _bingoEventQueue.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashIn), Times.Never());
        }
    }
}