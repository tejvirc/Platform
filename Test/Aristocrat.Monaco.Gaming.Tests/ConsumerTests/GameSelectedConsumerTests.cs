namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Consumers;
    using Contracts;
    using Contracts.Models;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    [TestClass]
    public class GameSelectedConsumerTests
    {
        private const int GameId = 7;
        private const int Denom = 5;
        private readonly IntPtr _bottomHwnd = new IntPtr(1);
        private readonly IntPtr _topHwnd = new IntPtr(2);
        private readonly IntPtr _virtualButtonDeckHwnd = new IntPtr(3);
        private readonly IntPtr _topperHwnd = new IntPtr(4);

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
        public void WhenGameServiceIsNullExpectException()
        {
            var consumer = new GameSelectedConsumer(null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var gameService = new Mock<IGameService>();

            var consumer = new GameSelectedConsumer(gameService.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeEventExpectTerminate()
        {
            var service = new Mock<IGameService>();

            var consumer = new GameSelectedConsumer(service.Object);

            consumer.Consume(
                new GameSelectedEvent(GameId, Denom,  String.Empty, false, _bottomHwnd, _topHwnd, _virtualButtonDeckHwnd, _topperHwnd));

            service.Verify(
                s => s.Initialize(
                    It.Is<GameInitRequest>(
                        r => r.GameId == GameId &&
                             r.Denomination == Denom &&
                             r.GameBottomHwnd == _bottomHwnd &&
                             r.GameTopHwnd == _topHwnd &&
                             r.GameVirtualButtonDeckHwnd == _virtualButtonDeckHwnd &&
                             r.GameTopperHwnd == _topperHwnd)));
        }
    }
}
