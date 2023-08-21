namespace Aristocrat.Monaco.Asp.Tests.Client.Consumers
{
    using Asp.Client.Consumers;
    using Asp.Client.Contracts;
    using Asp.Client.DataSources;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class DoorOpenMeteredEventConsumerTests
    {
        private Mock<IDoorsDataSource> _doorsDataSource;
        private DoorOpenMeteredEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _doorsDataSource = new Mock<IDoorsDataSource>(MockBehavior.Strict);
            _target = new DoorOpenMeteredEventConsumer(_doorsDataSource.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ConsumeTest()
        {

            _doorsDataSource.Setup(
                x => x.OnDoorOpenMeterChanged(It.Is<DoorOpenMeteredEvent>(ev => ev.LogicalId == (int)AspDoorLogicalId.Logic))).Verifiable();

            _target.Consume(new DoorOpenMeteredEvent((int)DoorLogicalId.Logic, true, true, "testLogicDoorName"));

            _doorsDataSource.Verify();
        }
    }
}
