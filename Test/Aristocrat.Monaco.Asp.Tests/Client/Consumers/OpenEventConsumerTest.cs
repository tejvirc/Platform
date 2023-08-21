namespace Aristocrat.Monaco.Asp.Tests.Client.Consumers
{
    using Asp.Client.Consumers;
    using Asp.Client.Contracts;
    using Asp.Client.DataSources;
    using Hardware.Contracts.Door;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using Test.Common;

    [TestClass]
    public class OpenEventConsumerTests
    {
        private Mock<ILogicSealDataSource> _logicSealDataSource;
        private Mock<IDoorsDataSource> _doorsDataSource;
        private OpenEventConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _logicSealDataSource = new Mock<ILogicSealDataSource>(MockBehavior.Strict);
            _doorsDataSource = new Mock<IDoorsDataSource>(MockBehavior.Strict);
            _target = new OpenEventConsumer(_logicSealDataSource.Object, _doorsDataSource.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullExceptionHandlerTest()
        {
            _target = new OpenEventConsumer(null, _doorsDataSource.Object);
        }

        [TestMethod]
        public void ConsumeTest()
        {

            _logicSealDataSource.Setup(
                x => x.HandleEvent(It.Is<OpenEvent>(ev => ev.LogicalId == (int)AspDoorLogicalId.Logic))).Verifiable();
            _doorsDataSource.Setup(
                x => x.OnDoorStatusChanged(It.Is<OpenEvent>(ev => ev.LogicalId == (int)AspDoorLogicalId.Logic))).Verifiable();

            _target.Consume(new OpenEvent((int)DoorLogicalId.Logic, "testLogicDoorName"));

            _logicSealDataSource.Verify();
            _doorsDataSource.Verify();
        }
    }
}
