namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Bingo.Consumers;
    using Commands;
    using Hardware.Contracts.Printer;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class PrintCompletedConsumerTests
    {
        private PrintCompletedConsumer _target;
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _consumerContext = new(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _commandHandlerFactory = new(MockBehavior.Default);
        private readonly PrintCompletedEvent _event = new();

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, DisplayName = "Command Handler Factory Null")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorParametersTest(bool nullEventBus, bool nullCommandFactory)
        {
            _ = CreateTarget(nullEventBus, nullCommandFactory);
        }

        [TestMethod]
        public async Task ConsumesTest()
        {
            _commandHandlerFactory
                .Setup(x => x.Execute(It.IsAny<ReportEgmStatusCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask).Verifiable();

            await _target.Consume(_event, CancellationToken.None);
            _commandHandlerFactory.Verify();
        }

        private PrintCompletedConsumer CreateTarget(bool nullEventBus = false, bool nullCommandFactory = false)
        {
            return new PrintCompletedConsumer(
                nullEventBus ? null : _eventBus.Object,
                _consumerContext.Object,
                nullCommandFactory ? null : _commandHandlerFactory.Object);
        }
    }
}