namespace Aristocrat.Monaco.G2S.Tests.Consumers
{
    using System;
    using Application.Contracts;
    using Aristocrat.G2S.Client;
    using Aristocrat.G2S.Client.Devices;
    using G2S.Consumers;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class SystemEnabledByOperatorConsumerTest
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
            var consumer = new SystemEnabledByOperatorConsumer(null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var egm = new Mock<IG2SEgm>();

            var consumer = new SystemEnabledByOperatorConsumer(egm.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeExpectRemoveCondition()
        {
            var egm = new Mock<IG2SEgm>();
            var device = new Mock<ICabinetDevice>();

            egm.Setup(e => e.GetDevice<ICabinetDevice>()).Returns(device.Object);
            device.SetupGet(d => d.DeviceClass).Returns("cabinet");

            var consumer = new SystemEnabledByOperatorConsumer(egm.Object);

            consumer.Consume(new SystemEnabledByOperatorEvent());

            device.Verify(d => d.RemoveCondition(device.Object, EgmState.OperatorLocked));
        }
    }
}