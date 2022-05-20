namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Bingo.Client.Messages;
    using Bingo.Consumers;
    using Commands;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BankBalanceChangedConsumerTest
    {
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new Mock<ISharedConsumer>(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _handler = new Mock<ICommandHandlerFactory>(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>(MockBehavior.Default);

        private BankBalanceChangedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, false)]
        [DataRow(false, true, false)]
        [DataRow(false, false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTest(
            bool nullEventBus,
            bool nullHandler,
            bool nullProperties)
        {
            _target = CreateTarget(nullEventBus, nullHandler, nullProperties);
        }

        [TestMethod]
        public async Task ConsumerTest()
        {
            const string serialNumber = "123";
            _properties.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(serialNumber);
            _handler.Setup(
                    x => x.Execute(
                        It.Is<StatusResponseMessage>(m => m.MachineSerial == serialNumber),
                        It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            await _target.Consume(new BankBalanceChangedEvent(123, 1234, Guid.Empty), CancellationToken.None);
            _handler.Verify();
        }

        private BankBalanceChangedConsumer CreateTarget(
            bool nullEventBus = false,
            bool nullHandler = false,
            bool nullProperties = false)
        {
            return new BankBalanceChangedConsumer(
                nullEventBus ? null : _eventBus.Object,
                _sharedConsumer.Object,
                nullProperties ? null : _properties.Object,
                nullHandler ? null : _handler.Object);
        }
    }
}