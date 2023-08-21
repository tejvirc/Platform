namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Sas.Contracts.Events;
    using Aristocrat.Sas.Client.LongPollDataClasses;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TransferLockConsumerTests
    {
        private TransferLockConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new TransferLockConsumer(
                _eventBus.Object,
                _consumerContext.Object,
                _reportingService.Object);
        }

        [DataRow(true, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, DisplayName = "Reporting Event Service Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool eventBusNull, bool reportingNull)
        {
            _target = new TransferLockConsumer(
                eventBusNull ? null : _eventBus.Object,
                _consumerContext.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [DataRow(true, false, ReportableEvent.TransferLock, DisplayName = "Lock Active, No Bonus")]
        [DataRow(false, false, ReportableEvent.TransferUnlock, DisplayName = "Lock Off, No Bonus")]
        [DataTestMethod]
        public void ConsumesTest(bool locked, bool bonus, ReportableEvent @event)
        {
            var transferCondition = bonus ? AftTransferConditions.BonusAwardToGamingMachineOk : AftTransferConditions.TransferToGamingMachineOk;
            var evt = new TransferLockEvent(locked, transferCondition);

            _target.Consume(evt);

            _reportingService.Verify(m => m.AddNewEventToQueue(@event), Times.Once());
        }
    }
}
