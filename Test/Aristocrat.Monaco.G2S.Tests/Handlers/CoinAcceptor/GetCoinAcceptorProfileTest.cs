namespace Aristocrat.Monaco.G2S.Tests.Handlers.CoinAcceptor
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.CoinAcceptor;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GetCoinAcceptorProfileTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEgmIsNullExpectExpection()
        {
            var handler = new GetCoinAcceptorProfile(null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public async Task WhenVerifyWithOwnerAndGuestsExpectNoError()
        {
            var egm = HandlerUtilities.CreateMockEgm<ICoinAcceptor>();

            var handler = new GetCoinAcceptorProfile(egm);

            await VerificationTests.VerifyChecksForOwnerAndGuests(handler);
        }

        [TestMethod]
        public async Task WhenHandleCommandWithValidParamsExpectSuccess()
        {
            var device = new Mock<ICoinAcceptor>();
            device.SetupGet(m => m.RestartStatus).Returns(true);
            device.SetupGet(m => m.UseDefaultConfig).Returns(true);
            device.SetupGet(m => m.RequiredForPlay).Returns(true);
            device.SetupGet(m => m.ConfigurationId).Returns(1);
            device.SetupGet(m => m.ConfigComplete).Returns(true);
            device.SetupGet(m => m.ConfigDateTime).Returns(DateTime.MaxValue);

            var egm = HandlerUtilities.CreateMockEgm(device);

            var handler = new GetCoinAcceptorProfile(egm);

            var command =
                ClassCommandUtilities.CreateClassCommand<coinAcceptor, getCoinAcceptorProfile>(
                    TestConstants.HostId,
                    TestConstants.EgmId);

            await handler.Handle(command);

            var response = command.Responses.First() as ClassCommand<coinAcceptor, coinAcceptorProfile>;

            Assert.AreEqual(true, response.Command.restartStatus);
            Assert.AreEqual(true, response.Command.useDefaultConfig);
            Assert.AreEqual(true, response.Command.requiredForPlay);
            Assert.AreEqual(1, response.Command.configurationId);
            Assert.AreEqual(true, response.Command.configComplete);
            Assert.AreEqual(t_g2sBoolean.G2S_false, response.Command.promoSupported);
            Assert.AreEqual(DateTime.MaxValue, response.Command.configDateTime);

            // TODO: Test coin data after GetCoinAcceptorProfile handler completion
            Assert.IsNotNull(response.Command.coinData);
        }
    }
}