namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Commands;
    using Common;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PrinterHardwareWarningConsumerTests
    {
        private PrinterHardwareWarningConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Default);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<ICommandHandlerFactory> _commandHandlerFactory = new(MockBehavior.Default);
        private readonly HardwareWarningEvent _event = new(PrinterWarningTypes.PaperLow);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, false, DisplayName = "EventBus Null")]
        [DataRow(false, false, true, DisplayName = "Command Handler Factory Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool nullReporting, bool nullEventBus, bool nullCommandFactory)
        {
            _ = CreateTarget(nullReporting, nullEventBus, nullCommandFactory);
        }

        [TestMethod]
        public async Task ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.PrinterPaperLow)).Verifiable();
            _commandHandlerFactory
                .Setup(x => x.Execute(It.IsAny<ReportEgmStatusCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask).Verifiable();

            await _target.Consume(_event, CancellationToken.None);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.PrinterPaperLow), Times.Once());
            _commandHandlerFactory.Verify();
        }

        [TestMethod]
        public async Task ConsumesNotPaperLowTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(It.IsAny<ReportableEvent>())).Verifiable();

            await _target.Consume(new HardwareWarningEvent(PrinterWarningTypes.PaperInChute), CancellationToken.None);

            _reportingService.Verify(m => m.AddNewEventToQueue(It.IsAny<ReportableEvent>()), Times.Never());
        }

        private PrinterHardwareWarningConsumer CreateTarget(
            bool nullReporting = false,
            bool nullEventBus = false,
            bool nullCommandFactory = false)
        {
            return new PrinterHardwareWarningConsumer(
                nullEventBus ? null : _eventBus.Object,
                _consumerContext.Object,
                nullReporting ? null : _reportingService.Object,
                nullCommandFactory ? null : _commandHandlerFactory.Object);
        }
    }
}