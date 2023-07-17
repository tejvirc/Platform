namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using Aristocrat.G2S.Protocol.v21;
    using G2S.Consumers;
    using G2S.Handlers;
    using G2S.Meters;
    using Gaming.Contracts;
    using Gaming.Contracts.Meters;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class PrimaryGameStartedConsumerTest
    {
        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEgmIsNullExpectException()
        {
            var consumer = new PrimaryGameStartedConsumer(null, null, null, null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenCommandBuilderIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();

            var consumer = new PrimaryGameStartedConsumer(egm.Object, null, null, null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPropertiesManagerIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();

            var consumer = new PrimaryGameStartedConsumer(egm.Object, builder.Object, null, null, null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenEventLiftIsNullExpectException()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var provider = new Mock<IGameProvider>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();

            var consumer = new PrimaryGameStartedConsumer(
                egm.Object,
                builder.Object,
                cabinetMeters.Object,
                provider.Object,
                null,
                null,
                null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var provider = new Mock<IGameProvider>();
            var history = new Mock<IGameHistory>();
            var lift = new Mock<IEventLift>();
            var gameMeters = new Mock<IGameMeterManager>();

            var consumer = new PrimaryGameStartedConsumer(
                egm.Object,
                builder.Object,
                cabinetMeters.Object,
                provider.Object,
                history.Object,
                gameMeters.Object,
                lift.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeExpectSetGamePlayDeviceAndRaiseEvent()
        {
            const int gameId = 1;
            const long denom = 25000L;
            const string wagerCategory = "1";
            const string themeId = @"ATI_TEST1234";
            const string paytableId = @"VAR01";

            var egm = new Mock<IG2SEgm>();
            var device = new Mock<ICabinetDevice>();
            var cabinetMeters = new Mock<IMeterAggregator<ICabinetDevice>>();
            var builder = new Mock<ICommandBuilder<ICabinetDevice, cabinetStatus>>();
            var provider = new Mock<IGameProvider>();
            var history = new Mock<IGameHistory>();
            var lift = new Mock<IEventLift>();
            var gameMeters = new Mock<IGameMeterManager>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");
            history.Setup(m => m.GetByIndex(It.IsAny<int>())).Returns(new Mock<IGameHistoryLog>().Object);

            provider.Setup(p => p.GetGame(gameId)).Returns(Factory_CreateMockGameProfile(gameId, themeId, paytableId));

            var consumer = new PrimaryGameStartedConsumer(
                egm.Object,
                builder.Object,
                cabinetMeters.Object,
                provider.Object,
                history.Object,
                gameMeters.Object,
                lift.Object);

            var log = new Mock<IGameHistoryLog>();
            log.Setup(m => m.ShallowCopy()).Returns(log.Object);
            consumer.Consume(new PrimaryGameStartedEvent(gameId, denom, wagerCategory, log.Object));

            lift.Verify(
                l => l.Report(
                    It.Is<IDevice>(d => device.Object == d),
                    It.Is<string>(e => e == EventCode.G2S_CBE314),
                    It.IsAny<deviceList1>(),
                    It.IsAny<IEvent>()));
        }

        private static IGameDetail Factory_CreateMockGameProfile(int gameId, string theme, string paytable)
        {
            var gameDetail = new Mock<IGameDetail>();

            gameDetail.SetupGet(g => g.Id).Returns(gameId);
            gameDetail.SetupGet(g => g.ThemeId).Returns(theme);
            gameDetail.SetupGet(g => g.PaytableId).Returns(paytable);
            gameDetail.SetupGet(g => g.ActiveDenominations).Returns(new List<long>());

            return gameDetail.Object;
        }
    }
}