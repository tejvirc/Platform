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
    public class PrinterHardwareFaultClearConsumerTests
    {
        private PrinterHardwareFaultClearConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<ICommandHandlerFactory> _commandHandlerFactory = new(MockBehavior.Default);

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

        [DataRow(PrinterFaultTypes.PaperEmpty, ReportableEvent.PrinterPaperOutClear, true, DisplayName = "Paper Inserted")]
        [DataRow(PrinterFaultTypes.PaperNotTopOfForm, ReportableEvent.PrinterMediaLoaded, false, DisplayName = "Paper Top of form")]
        [DataRow(PrinterFaultTypes.PrintHeadDamaged, ReportableEvent.PrinterErrorClear, false, DisplayName = "Printer Error Clear")]
        [DataTestMethod]
        public async Task ConsumesTest(PrinterFaultTypes fault, ReportableEvent response, bool sendStatus)
        {
            var @event = new HardwareFaultClearEvent(fault);
            _reportingService.Setup(m => m.AddNewEventToQueue(response)).Verifiable();
            if (sendStatus)
            {
                _commandHandlerFactory
                    .Setup(x => x.Execute(It.IsAny<ReportEgmStatusCommand>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask).Verifiable();
            }

            await _target.Consume(@event, CancellationToken.None);

            _reportingService.Verify(m => m.AddNewEventToQueue(response), Times.Once());
            if (sendStatus)
            {
                _commandHandlerFactory.Verify();
            }
        }

        private PrinterHardwareFaultClearConsumer CreateTarget(
            bool nullReporting = false,
            bool nullEventBus = false,
            bool nullCommandFactory = false)
        {
            return new PrinterHardwareFaultClearConsumer(
                nullEventBus ? null : _eventBus.Object,
                _consumerContext.Object,
                nullReporting ? null : _reportingService.Object,
                nullCommandFactory ? null : _commandHandlerFactory.Object);
        }
    }
}