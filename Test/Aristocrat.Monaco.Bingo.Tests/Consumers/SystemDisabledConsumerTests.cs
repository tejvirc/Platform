namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SystemDisabledConsumerTests
    {
        private SystemDisabledConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Loose);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Strict);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<ISystemDisableManager> _systemDisable = new Mock<ISystemDisableManager>();
        private readonly SystemDisabledEvent _event = new();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new SystemDisabledConsumer(_eventBus.Object, _sharedConsumer.Object, _reportingService.Object);
            _systemDisable.Setup(mock => mock.CurrentDisableKeys).Returns(new List<Guid>() { new Guid("{F1BE3145-DF51-4C43-BAB6-F0E934681C74}") });
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new SystemDisabledConsumer(
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object,
                reportingNull ? null : _reportingService.Object);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.Disabled)).Verifiable();

            _target.Consume(_event);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.Disabled), Times.Once());
        }
    }
}