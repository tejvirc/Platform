namespace Aristocrat.Monaco.Bingo.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Bingo.Consumers;
    using Commands;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class TransactionCompletedConsumerTest
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _handler = new(MockBehavior.Default);

        private TransactionCompletedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTest(
            bool nullEventBus,
            bool nullHandler)
        {
            _target = CreateTarget(nullEventBus, nullHandler);
        }

        [TestMethod]
        public async Task ConsumerTest()
        {
            _handler.Setup(x => x.Execute(It.IsAny<ReportEgmStatusCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask).Verifiable();

            await _target.Consume(new TransactionCompletedEvent(), CancellationToken.None);
            _handler.Verify();
        }

        private TransactionCompletedConsumer CreateTarget(
            bool nullEventBus = false,
            bool nullHandler = false)
        {
            return new TransactionCompletedConsumer(
                nullEventBus ? null : _eventBus.Object,
                _sharedConsumer.Object,
                nullHandler ? null : _handler.Object);
        }
    }
}