namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Aristocrat.G2S;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices.v21;
    using Aristocrat.G2S.Protocol.v21;
    using Common.Events;
    using Data.CommConfig;
    using G2S.Consumers;
    using G2S.Handlers.CommConfig;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class CommHostListChangedConsumerTest
    {
        [TestInitialize]
        public void Initialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullEgmExpectExpection()
        {
            var commandBuilderMock = new Mock<ICommHostListCommandBuilder>();

            var consumer = new CommHostListChangedConsumer(null, commandBuilderMock.Object);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenConstructWithNullCommandBuilderExpectExpection()
        {
            var egmMock = new Mock<IG2SEgm>();

            var consumer = new CommHostListChangedConsumer(egmMock.Object, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeWithValidDataExpectSuccess()
        {
            var egmMock = new Mock<IG2SEgm>();
            var commandBuilderMock = new Mock<ICommHostListCommandBuilder>();

            var deviceMock = new Mock<ICommConfigDevice>();
            deviceMock.SetupGet(m => m.DeviceClass).Returns(DeviceClass.G2S_commConfig);
            egmMock.Setup(m => m.GetDevice<ICommConfigDevice>()).Returns(deviceMock.Object);

            var consumer = new CommHostListChangedConsumer(
                egmMock.Object,
                commandBuilderMock.Object);

            consumer.Consume(new CommHostListChangedEvent(new[] { 1 }));

            commandBuilderMock
                .Verify(
                    m => m.Build(
                        deviceMock.Object,
                        It.IsAny<commHostList>(),
                        It.IsAny<CommHostListCommandBuilderParameters>()));
            deviceMock.Verify(m => m.UpdateHostList(It.IsAny<commHostList>()));
        }
    }
}