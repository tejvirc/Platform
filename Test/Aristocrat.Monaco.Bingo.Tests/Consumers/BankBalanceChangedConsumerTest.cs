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
    public class BankBalanceChangedConsumerTest
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<ISharedConsumer> _sharedConsumer = new(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _handler = new(MockBehavior.Default);

        private BankBalanceChangedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentTest(bool nullEventBus, bool nullHandler)
        {
            _target = CreateTarget(nullEventBus, nullHandler);
        }

        [TestMethod]
        public async Task ConsumerTest()
        {
            _handler.Setup(x => x.Execute(It.IsAny<ReportEgmStatusCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask).Verifiable();

            await _target.Consume(new BankBalanceChangedEvent(123, 1234, Guid.Empty), CancellationToken.None);
            _handler.Verify();
        }

        private BankBalanceChangedConsumer CreateTarget(
            bool nullEventBus = false,
            bool nullHandler = false)
        {
            return new BankBalanceChangedConsumer(
                nullEventBus ? null : _eventBus.Object,
                _sharedConsumer.Object,
                nullHandler ? null : _handler.Object);
        }
    }
}