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
    public class PrinterHardwareFaultConsumerTests
    {
        private PrinterHardwareFaultConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Default);
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

        [DataRow(PrinterFaultTypes.PrintHeadOpen, ReportableEvent.PrinterError, false, DisplayName = "Print Head Open")]
        [DataRow(PrinterFaultTypes.ChassisOpen, ReportableEvent.PrinterError, false, DisplayName = "Print Chassis Open")]
        [DataRow(PrinterFaultTypes.OtherFault, ReportableEvent.PrinterError, false, DisplayName = "Printer Other Fault")]
        [DataRow(PrinterFaultTypes.TemperatureFault, ReportableEvent.PrinterHardwareFailure, false, DisplayName = "Temperature Fault")]
        [DataRow(PrinterFaultTypes.PrintHeadDamaged, ReportableEvent.PrinterHardwareFailure, false, DisplayName = "Print Head Damaged")]
        [DataRow(PrinterFaultTypes.NvmFault, ReportableEvent.PrinterHardwareFailure, false, DisplayName = "Printer NvRam Fault")]
        [DataRow(PrinterFaultTypes.FirmwareFault, ReportableEvent.PrinterHardwareFailure, false, DisplayName = "Printer Firmware Fault")]
        [DataRow(PrinterFaultTypes.PaperJam, ReportableEvent.PrinterMediaJam, false, DisplayName = "Paper Jam")]
        [DataRow(PrinterFaultTypes.PaperEmpty, ReportableEvent.PrinterPaperOut, true, DisplayName = "Paper Empty")]
        [DataRow(PrinterFaultTypes.PaperNotTopOfForm, ReportableEvent.PrinterMediaMissingIndexMark, false, DisplayName = "Paper Not Top of form")]
        [DataTestMethod]
        public async Task ConsumesTest(PrinterFaultTypes fault, ReportableEvent response, bool sendStatus)
        {
            var @event = new HardwareFaultEvent(fault);
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

        private PrinterHardwareFaultConsumer CreateTarget(
            bool nullReporting = false,
            bool nullEventBus = false,
            bool nullCommandFactory = false)
        {
            return new PrinterHardwareFaultConsumer(
                nullEventBus ? null : _eventBus.Object,
                _consumerContext.Object,
                nullReporting ? null : _reportingService.Object,
                nullCommandFactory ? null : _commandHandlerFactory.Object);
        }
    }
}