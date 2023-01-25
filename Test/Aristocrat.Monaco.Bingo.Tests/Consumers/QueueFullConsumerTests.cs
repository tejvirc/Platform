namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Bingo.Consumers;
    using Commands;
    using Common.Events;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class QueueFullConsumerTest
    {
        private QueueFullConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _commandHandlerFactory = new(MockBehavior.Default);

        private readonly QueueFullEvent _event = new();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, false, DisplayName = "Properties Manager Null")]
        [DataRow(false, false, true, DisplayName = "Command Handler Factory Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool nullEventBus, bool nullProperties, bool nullCommandFactory)
        {
            _ = CreateTarget(nullEventBus, nullProperties, nullCommandFactory);
        }

        [TestMethod]
        public async Task ConsumesTest()
        {
            const string serialNumber = "TestingSerialNumber";
            _commandHandlerFactory
                .Setup(
                    x => x.Execute(
                        It.Is<StatusResponseMessage>(s => s.MachineSerial == serialNumber),
                        It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            _properties.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, string.Empty))
                .Returns(serialNumber);

            await _target.Consume(_event, CancellationToken.None);
            _commandHandlerFactory.Verify();
        }

        private QueueFullConsumer CreateTarget(
            bool nullEventBus = false,
            bool nullProperties = false,
            bool nullCommandFactory = false)
        {
            return new QueueFullConsumer(
                nullEventBus ? null : _eventBus.Object,
                _consumerContext.Object,
                nullProperties ? null : _properties.Object,
                nullCommandFactory ? null : _commandHandlerFactory.Object);
        }
    }
}