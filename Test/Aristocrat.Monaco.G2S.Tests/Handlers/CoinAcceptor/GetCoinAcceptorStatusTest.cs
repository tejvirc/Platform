namespace Aristocrat.Monaco.G2S.Tests.Handlers.CoinAcceptor
{
    using System;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers;
    using G2S.Handlers.CoinAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetCoinAcceptorStatusTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectException()
        {
            var commandBuilderMock = new Mock<ICommandBuilder<ICoinAcceptor, coinAcceptorStatus>>();

            var handler = new GetCoinAcceptorStatus(null, commandBuilderMock.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectException()
        {
            var egmMock = new Mock<IG2SEgm>();

            var handler = new GetCoinAcceptorStatus(egmMock.Object, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerAndGuestsExpectNoError()
        {
            var commandBuilderMock = new Mock<ICommandBuilder<ICoinAcceptor, coinAcceptorStatus>>();
            var egm = HandlerUtilities.CreateMockEgm<ICoinAcceptor>();

            var handler = new GetCoinAcceptorStatus(egm, commandBuilderMock.Object);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenHandleWithValidParamsExpectSuccess()
        {
            var commandBuilderMock = new Mock<ICommandBuilder<ICoinAcceptor, coinAcceptorStatus>>();
            var egm = HandlerUtilities.CreateMockEgm<ICoinAcceptor>();

            var handler = new GetCoinAcceptorStatus(egm, commandBuilderMock.Object);

            var command =
                ClassCommandUtilities.CreateClassCommand<coinAcceptor, getCoinAcceptorStatus>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            commandBuilderMock
                .Verify(m => m.Build(It.IsAny<ICoinAcceptor>(), It.IsAny<coinAcceptorStatus>()), Times.Once);
        }
    }
}