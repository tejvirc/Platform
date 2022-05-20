namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using Contracts;
    using Gaming.Commands;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BeginGameRoundCommandHandlerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenPersistentStorageIsNullExpectException()
        {
            var handler = new BeginGameRoundCommandHandler(null, null, null, null);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var gameState = new Mock<IGamePlayState>();
            var recovery = new Mock<IGameRecovery>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            var handler = new BeginGameRoundCommandHandler(
                recovery.Object,
                gameState.Object,
                properties.Object,
                bus.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenHandleWithSuccessfulIntegrityCheckExpectStart()
        {
            const long denom = 1L;

            var gameState = new Mock<IGamePlayState>();
            var recovery = new Mock<IGameRecovery>();
            var properties = new Mock<IPropertiesManager>();
            var bus = new Mock<IEventBus>();

            properties.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>())).Returns(denom);

            var handler = new BeginGameRoundCommandHandler(
                recovery.Object,
                gameState.Object,
                properties.Object,
                bus.Object);

            handler.Handle(new BeginGameRound(denom));
        }
    }
}