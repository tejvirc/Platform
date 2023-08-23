namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Hardware.Contracts.NoteAcceptor;
    using Bingo.Consumers;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;

    [TestClass]
    public class NoteAcceptorHardwareFaultConsumerTests
    {
        private NoteAcceptorHardwareFaultConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Default);
        private readonly Mock<IReportTransactionQueueService> _bingoTransactionReportHandler = new(MockBehavior.Default);
        private readonly Mock<IMeterManager> _meterManager = new(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _propertiesManager = new(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new NoteAcceptorHardwareFaultConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _reportingService.Object,
                _bingoTransactionReportHandler.Object,
                _meterManager.Object,
                _unitOfWorkFactory.Object,
                _gameProvider.Object);

            _propertiesManager.Setup(x => x.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(false);
        }

        [DataRow(true, false, false, false, false, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, false, false, false, false, DisplayName = "EventBus Null")]
        [DataRow(false, false, true, false, false, false, DisplayName = "Transaction Reporting Null")]
        [DataRow(false, false, false, true, false, false, DisplayName = "Meter Manager Null")]
        [DataRow(false, false, false, false, true, false, DisplayName = "Unit of Work Factory Null")]
        [DataRow(false, false, false, false, false, true, DisplayName = "Properties manager Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(
            bool reportingNull,
            bool eventBusNull,
            bool transactionNull,
            bool meterNull,
            bool nullUnitOfWork,
            bool nullGameProvider)
        {
            _target = new NoteAcceptorHardwareFaultConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object,
                transactionNull ? null : _bingoTransactionReportHandler.Object,
                meterNull ? null : _meterManager.Object,
                nullUnitOfWork ? null : _unitOfWorkFactory.Object,
                nullGameProvider ? null : _gameProvider.Object);
        }

        [DataRow(NoteAcceptorFaultTypes.FirmwareFault, ReportableEvent.BillAcceptorSoftwareError, DisplayName = "Firmware Fault")]
        [DataRow(NoteAcceptorFaultTypes.OpticalFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "Optical Fault")]
        [DataRow(NoteAcceptorFaultTypes.ComponentFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "Component Fault")]
        [DataRow(NoteAcceptorFaultTypes.NvmFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "NvRam Fault")]
        [DataRow(NoteAcceptorFaultTypes.StackerFull, ReportableEvent.BillAcceptorStackerIsFull, DisplayName = "Stacker Full Fault")]
        [DataRow(NoteAcceptorFaultTypes.StackerFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "Stacker Fault")]
        [DataRow(NoteAcceptorFaultTypes.CheatDetected, ReportableEvent.BillAcceptorCheatDetected, DisplayName = "Cheat Detected Fault")]
        [DataRow(NoteAcceptorFaultTypes.OtherFault, ReportableEvent.BillAcceptorError, DisplayName = "Other Fault")]
        [DataRow(NoteAcceptorFaultTypes.MechanicalFault, ReportableEvent.BillAcceptorHardwareFailure, DisplayName = "Mechanical Fault")]
        [DataRow(NoteAcceptorFaultTypes.StackerJammed, ReportableEvent.BillAcceptorStackerJammed, DisplayName = "Stacker Jammed Fault")]
        [DataRow(NoteAcceptorFaultTypes.NoteJammed, ReportableEvent.BillAcceptorStackerJammed, DisplayName = "Note Jammed Fault")]
        [DataTestMethod]
        public void ConsumesTest(NoteAcceptorFaultTypes fault, ReportableEvent response)
        {
            var @event = new HardwareFaultEvent(fault);
            _target.Consume(@event);

            if (response is not ReportableEvent.BillAcceptorError)
            {
                _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.BillAcceptorError), Times.Once());
            }

            _reportingService.Verify(m => m.AddNewEventToQueue(response), Times.Once());
        }

        [TestMethod]
        public void ConsumesNoneTest()
        {
            _target.Consume(new(NoteAcceptorFaultTypes.None));
            _reportingService.Verify(m => m.AddNewEventToQueue(It.IsAny<ReportableEvent>()), Times.Never());
        }

        [TestMethod]
        public void ConsumesStackerDisconnectedTest()
        {
            var periodValue = 1234000;
            var meter = new Mock<IMeter>(MockBehavior.Strict);
            meter.Setup(m => m.Period).Returns(periodValue);

            _meterManager.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(meter.Object);
            _target.Consume(new(NoteAcceptorFaultTypes.StackerDisconnected));

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.BillAcceptorError), Times.Once());
            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.StackerRemoved), Times.Once());
            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.CashDrop), Times.Once());
            _bingoTransactionReportHandler.Verify(m => m.AddNewTransactionToQueue(TransactionType.Drop, 1234, 0, 0, 0, 0, string.Empty), Times.Once());
        }
    }
}