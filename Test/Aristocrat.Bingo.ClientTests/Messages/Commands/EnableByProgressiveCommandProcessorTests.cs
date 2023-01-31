namespace Aristocrat.Bingo.Client.Tests.Messages.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Messages;
    using Client.Messages.Progressives;
    using Client.Messages.Commands;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using ServerApiGateway;

    [TestClass]
    public class EnableByProgressiveCommandProcessorTests
    {
        private EnableByProgressiveCommandProcessor _target;

        private readonly Mock<IMessageHandlerFactory> _messageHandler =
            new Mock<IMessageHandlerFactory>(MockBehavior.Default);

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new EnableByProgressiveCommandProcessor(_messageHandler.Object);
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullConstructorArgumentsTest()
        {
            _target = new EnableByProgressiveCommandProcessor(null);
        }

        [DataRow(ResponseCode.Ok, true)]
        [DataRow(ResponseCode.Cancelled, false)]
        [DataRow(ResponseCode.Disconnected, false)]
        [DataRow(ResponseCode.Rejected, false)]
        [DataTestMethod]
        public async Task ProcessTest(ResponseCode code, bool successful)
        {
            _messageHandler
                .Setup(x => x.Handle<ProgressiveUpdateResponse, DisableByProgressiveMessage>(It.IsAny<DisableByProgressiveMessage>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ProgressiveUpdateResponse(code)));

            var progressiveUpdate = new ProgressiveUpdate
            {
                ProgressiveMeta = Any.Pack(new EnableByProgressive())
            };

            var result = await _target.ProcessCommand(progressiveUpdate, CancellationToken.None);

            Assert.AreEqual(result, null);
        }
    }
}