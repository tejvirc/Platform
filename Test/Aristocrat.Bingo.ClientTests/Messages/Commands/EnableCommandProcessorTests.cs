namespace Aristocrat.Bingo.Client.Tests.Messages.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Messages;
    using Client.Messages.Commands;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;

    [TestClass]
    public class EnableCommandProcessorTests
    {
        private EnableCommandProcessor _target;

        private readonly Mock<IMessageHandlerFactory> _messageHandler =
            new Mock<IMessageHandlerFactory>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new EnableCommandProcessor(_messageHandler.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullConstructorArgumentsTest()
        {
            _target = new EnableCommandProcessor(null);
        }

        [DataRow(ResponseCode.Ok, true)]
        [DataRow(ResponseCode.Cancelled, false)]
        [DataRow(ResponseCode.Disconnected, false)]
        [DataRow(ResponseCode.Rejected, false)]
        [DataTestMethod]
        public async Task ProcessTest(ResponseCode code, bool successful)
        {
            _messageHandler
                .Setup(x => x.Handle<EnableResponse, Enable>(It.IsAny<Enable>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new EnableResponse(code)));
            var result = await _target.ProcessCommand(
                new Command { Command_ = Any.Pack(new EnableCommand()) },
                CancellationToken.None);
            var response = result as EnableCommandResponse;
            Assert.IsNotNull(response, "The response is not a EnableCommandResponse");
            Assert.AreEqual(response.Status, successful);
        }
    }
}