namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Bingo.Common;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Aristocrat.Monaco.Test.Common;
    using Bingo.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ReelHardwareFaultClearConsumerTests
    {
        private ReelHardwareFaultClearConsumer _target;
        private Mock<IServiceManager> _serviceManager;
        private Mock<IReelController> _reelController;
        private readonly Mock<IEventBus> _eventBus = new (MockBehavior.Loose);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _serviceManager = MoqServiceManager.CreateInstance(MockBehavior.Strict);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);
            _reelController.Setup(mock => mock.Faults).Returns(new Dictionary<int, ReelFaults>() { { 0, ReelFaults.None } });

            _target = new ReelHardwareFaultClearConsumer(_eventBus.Object, _sharedConsumer.Object, _reportingService.Object);
        }

        /// <summary>
        ///     Cleans up class members after execution of a TestMethod.
        /// </summary>
        [TestCleanup]
        public void CleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, DisplayName = "EventBus Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool reportingNull, bool eventBusNull)
        {
            _target = new ReelHardwareFaultClearConsumer(
                eventBusNull ? null : _eventBus.Object,
                _sharedConsumer.Object,
                reportingNull ? null : _reportingService.Object);

        }

        [TestMethod]
        public void FaultClearWithNoFaultsTest()
        {
            _reelController.Setup(mock => mock.ReelControllerFaults).Returns(ReelControllerFaults.None);
            _reelController.Setup(mock => mock.Faults).Returns(new Dictionary<int, ReelFaults>() { { 1, ReelFaults.None }, { 2, ReelFaults.Disconnected }});
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.ReelErrorClear)).Verifiable();

            _target.Consume(new HardwareReelFaultClearEvent(ReelFaults.None));

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.ReelErrorClear), Times.Once());
        }

        [TestMethod]
        public void FaultClearWithReelControllerFaultTest()
        {
            _reelController.Setup(mock => mock.ReelControllerFaults).Returns(ReelControllerFaults.LowVoltage);
            _reelController.Setup(mock => mock.Faults).Returns(new Dictionary<int, ReelFaults>() { { 1, ReelFaults.None } });
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.ReelErrorClear)).Verifiable();

            _target.Consume(new HardwareReelFaultClearEvent(ReelFaults.LowVoltage));

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.ReelErrorClear), Times.Never());
        }

        [TestMethod]
        public void FaultClearWithReelFaultsTest()
        {
            _reelController.Setup(mock => mock.ReelControllerFaults).Returns(ReelControllerFaults.None);
            _reelController.Setup(mock => mock.Faults).Returns(new Dictionary<int, ReelFaults>() { { 1, ReelFaults.None }, { 2, ReelFaults.ReelStall } });
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.ReelErrorClear)).Verifiable();

            _target.Consume(new HardwareReelFaultClearEvent(ReelFaults.ReelStall));

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.ReelErrorClear), Times.Never());
        }
    }
}