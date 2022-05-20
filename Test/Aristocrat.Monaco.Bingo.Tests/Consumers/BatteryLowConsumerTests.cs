namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Monaco.Bingo.Services.Reporting;
    using Bingo.Consumers;
    using Commands;
    using Common;
    using Hardware.Contracts.Battery;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BatteryLowConsumerTests
    {
        private BatteryLowConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Default);
        private readonly Mock<IReportEventQueueService> _reportingService = new(MockBehavior.Strict);
        private readonly Mock<IPropertiesManager> _properties = new(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _commandHandlerFactory = new(MockBehavior.Default);

        private readonly BatteryLowEvent _event = new(1);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, false, DisplayName = "Reporting Service Null")]
        [DataRow(false, true, false, false, DisplayName = "EventBus Null")]
        [DataRow(false, false, true, false, DisplayName = "Properties Manager Null")]
        [DataRow(false, false, false, true, DisplayName = "Command Handler Factory Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool nullReporting, bool nullEventBus, bool nullProperties, bool nullCommandFactory)
        {
            _ = CreateTarget(nullReporting, nullEventBus, nullProperties, nullCommandFactory);
        }

        [TestMethod]
        public async Task ConsumesTest()
        {
            const string serialNumber = "TestingSerialNumber";
            _reportingService.Setup(m => m.AddNewEventToQueue(ReportableEvent.NvRamBatteryLow)).Verifiable();
            _commandHandlerFactory
                .Setup(
                    x => x.Execute(
                        It.Is<StatusResponseMessage>(s => s.MachineSerial == serialNumber),
                        It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            _properties.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, string.Empty))
                .Returns(serialNumber);
            await _target.Consume(_event, CancellationToken.None);

            _reportingService.Verify(m => m.AddNewEventToQueue(ReportableEvent.NvRamBatteryLow), Times.Once());
            _commandHandlerFactory.Verify();
        }

        private BatteryLowConsumer CreateTarget(
            bool nullReporting = false,
            bool nullEventBus = false,
            bool nullProperties = false,
            bool nullCommandFactory = false)
        {
            return new BatteryLowConsumer(
                nullEventBus ? null : _eventBus.Object,
                _consumerContext.Object,
                nullReporting ? null : _reportingService.Object,
                nullProperties ? null : _properties.Object,
                nullCommandFactory ? null : _commandHandlerFactory.Object);
        }
    }
}