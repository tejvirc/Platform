namespace Aristocrat.Bingo.Client.Tests.Messages.Commands
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Client.Messages;
    using Client.Messages.Commands;
    using Google.Protobuf.WellKnownTypes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ServerApiGateway;

    [TestClass]
    public class PingCommandProcessorTests
    {
        private PingCommandProcessor _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new PingCommandProcessor();
        }

        [TestMethod]
        public async Task ProcessTest()
        {
            var pingCommand = new PingCommand { PingRequestTime = Timestamp.FromDateTime(DateTime.UtcNow) };
            var result = await _target.ProcessCommand(
                new Command { Command_ = Any.Pack(pingCommand) },
                CancellationToken.None);
            var response = result as PingResponse;
            Assert.IsNotNull(response, "The response is not a PingResponse");
            Assert.AreEqual(response.PingRequestTime, pingCommand.PingRequestTime);
        }
    }
}