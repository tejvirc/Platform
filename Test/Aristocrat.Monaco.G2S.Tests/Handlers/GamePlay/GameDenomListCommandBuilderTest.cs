namespace Aristocrat.Monaco.G2S.Tests.Handlers.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Handlers.GamePlay;
    using Gaming.Contracts;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameDenomListCommandBuilderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullGameProviderExpectException()
        {
            var builder = new GameDenomListCommandBuilder(null);
            Assert.IsNull(builder);
        }

        [TestMethod]
        public async Task WhenHandleCommandExpectSuccess()
        {
            var deviceId = 1;

            var gameProfile = new Mock<IGameDetail>();
            gameProfile.SetupGet(x => x.SupportedDenominations).Returns(new List<long> { 5 });
            gameProfile.SetupGet(x => x.ActiveDenominations).Returns(new List<long> { 4 });

            var gameProvider = new Mock<IGameProvider>();
            gameProvider.Setup(x => x.GetGame(deviceId)).Returns(gameProfile.Object);

            var builder = new GameDenomListCommandBuilder(gameProvider.Object);

            var command = new gameDenomList();
            Assert.IsNull(command.Items);

            var device = new Mock<IGamePlayDevice>();
            device.SetupGet(x => x.Id).Returns(deviceId);

            await builder.Build(device.Object, command);
            Assert.IsNotNull(command.Items);
            Assert.AreEqual(command.Items.Length, 1);
            Assert.IsTrue(command.Items[0] is gameDenom);
            Assert.IsTrue(((gameDenom)command.Items[0]).denomId == 5);
            Assert.IsFalse(((gameDenom)command.Items[0]).active);
        }
    }
}