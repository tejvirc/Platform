namespace Aristocrat.Monaco.Mgam.Tests.Consumers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Aristocrat.Mgam.Client.Logging;
    using Aristocrat.Mgam.Client.Messaging;
    using Commands;
    using Mgam.Consumers;
    using Hardware.Contracts.Ticket;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using VoucherPrinted = Commands.VoucherPrinted;

    [TestClass]
    public class VoucherIssuedConsumerTest
    {
        private Mock<ILogger<VoucherIssuedConsumer>> _logger;
        private Mock<ICommandHandlerFactory> _commandFactory;
        private VoucherIssuedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _logger = new Mock<ILogger<VoucherIssuedConsumer>>(MockBehavior.Default);
            _commandFactory = new Mock<ICommandHandlerFactory>(MockBehavior.Default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullCommandFactoryTest()
        {
            _target = new VoucherIssuedConsumer(
                null,
                null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullELoggerTest()
        {
            _target = new VoucherIssuedConsumer(
                _commandFactory.Object,
                null);
        }

        [TestMethod]
        public void SuccessTest()
        {
            _target = new VoucherIssuedConsumer(
                _commandFactory.Object,
                _logger.Object);

            Assert.IsNotNull(_target);
        }

        [TestMethod]
        public void SuccessConsumer()
        {
            _target = new VoucherIssuedConsumer(
                _commandFactory.Object,
                _logger.Object);

            _commandFactory.Setup(c => c.Execute(It.IsAny<VoucherPrinted>())).Returns(Task.CompletedTask).Verifiable();

            _target.Consume(
                new VoucherIssuedEvent(
                    new VoucherOutTransaction(1, DateTime.Today, 0, AccountType.Cashable, "1234", 0, 0, null),
                    new Ticket()),
                CancellationToken.None).Wait(10);

            _commandFactory.Verify();
        }

        [TestMethod]
        public void FailConsumer()
        {
            _target = new VoucherIssuedConsumer(
                _commandFactory.Object,
                _logger.Object);

            _commandFactory.Setup(c => c.Execute(It.IsAny<VoucherPrinted>()))
                .Throws(new ServerResponseException(ServerResponseCode.ServerError)).Verifiable();

            _target.Consume(
                new VoucherIssuedEvent(
                    new VoucherOutTransaction(1, DateTime.Today, 0, AccountType.Cashable, "1234", 0, 0, null),
                    new Ticket()),
                CancellationToken.None).Wait(10);

            _commandFactory.Verify();
        }
    }
}