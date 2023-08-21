namespace Aristocrat.Monaco.Gaming.Tests.ConsumerTests
{
    using System;
    using Consumers;
    using Contracts;
    using Gaming.Commands;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Test.Common;

    /// <summary>
    ///     Summary description for EndGameProcessConsumerTests
    /// </summary>
    [TestClass]
    public class GameProcessExitedConsumerTests
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
        public void WhenGameServiceIsNullExpectException()
        {
            var consumer = new GameProcessExitedConsumer(null, null, null);

            Assert.IsNull(consumer);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var gameService = new Mock<IGameService>();
            var bank = new Mock<IPlayerBank>();
            var handlerFactory = new Mock<ICommandHandlerFactory>();

            var consumer = new GameProcessExitedConsumer(gameService.Object, bank.Object, handlerFactory.Object);

            Assert.IsNotNull(consumer);
        }

        [TestMethod]
        public void WhenConsumeWithExpectedExpectShutdown()
        {
            var gameService = new Mock<IGameService>();
            var bank = new Mock<IPlayerBank>();
            var handlerFactory = new Mock<ICommandHandlerFactory>();

            handlerFactory.Setup(m => m.Create<ClearSessionData>())
                .Returns(new Mock<ICommandHandler<ClearSessionData>>().Object);

            var consumer = new GameProcessExitedConsumer(gameService.Object, bank.Object, handlerFactory.Object);

            consumer.Consume(new GameProcessExitedEvent(1));

            gameService.Verify(s => s.ShutdownEnd());
        }

        [TestMethod]
        public void WhenConsumeWithUnexpectedExpectTerminate()
        {
            var gameService = new Mock<IGameService>();
            var bank = new Mock<IPlayerBank>();
            var handlerFactory = new Mock<ICommandHandlerFactory>();

            handlerFactory.Setup(m => m.Create<ClearSessionData>())
                .Returns(new Mock<ICommandHandler<ClearSessionData>>().Object);

            var consumer = new GameProcessExitedConsumer(gameService.Object, bank.Object, handlerFactory.Object);

            consumer.Consume(new GameProcessExitedEvent(1, true));

            gameService.Verify(s => s.Terminate(1, false));
        }
    }
}