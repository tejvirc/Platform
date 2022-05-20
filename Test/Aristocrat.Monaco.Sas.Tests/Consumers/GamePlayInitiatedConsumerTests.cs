namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Test.Common;

    [TestClass]
    public class GamePlayInitiatedConsumerTests
    {
        private GamePlayInitiatedConsumer _target;
        private Mock<IPropertiesManager> _propertiesManager;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);

            _propertiesManager = new Mock<IPropertiesManager>(MockBehavior.Default);
            _target = new GamePlayInitiatedConsumer(_propertiesManager.Object);
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
            _target = new GamePlayInitiatedConsumer(null);
        }

        [TestMethod]
        public void ConsumesTest()
        {
            _propertiesManager.Setup(m => m.SetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>())).Verifiable();

            _target.Consume(new GamePlayInitiatedEvent());

            _propertiesManager.Verify(m => m.SetProperty(GamingConstants.PokerHandInformation, It.IsAny<HandInformation>()), Times.Once);
        }
    }
}