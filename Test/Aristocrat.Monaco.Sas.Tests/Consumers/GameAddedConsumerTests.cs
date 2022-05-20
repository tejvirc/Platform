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
    public class GameAddedConsumerTests
    {
        private Mock<IGameProvider> _gameProvider;
        private GameAddedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);

            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _target = new GameAddedConsumer(_gameProvider.Object);
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            _target.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        [TestMethod]
        public void NullGameProviderTest()
        {
            _target = new GameAddedConsumer(null);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            const int gameId = 1;
            _gameProvider.Setup(x => x.EnableGame(gameId, GameStatus.DisabledByBackend)).Verifiable();
            _target.Consume(new GameAddedEvent(gameId, string.Empty));

            _gameProvider.Verify();
        }
    }
}