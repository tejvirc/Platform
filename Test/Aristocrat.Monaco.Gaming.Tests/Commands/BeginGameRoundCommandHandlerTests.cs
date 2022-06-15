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
        private readonly Mock<IGameRecovery> _recovery = new Mock<IGameRecovery>();
        private readonly Mock<IGameDiagnostics> _diagnostics = new Mock<IGameDiagnostics>();
        private readonly Mock<IGamePlayState> _gameState = new Mock<IGamePlayState>();
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>();
        private readonly Mock<IEventBus> _eventBus = new Mock<IEventBus>();
        private readonly Mock<IGameStartConditionProvider> _conditions = new Mock<IGameStartConditionProvider>();

        [DataRow(true, false, false, false, false, false)]
        [DataRow(false, true, false, false, false, false)]
        [DataRow(false, false, true, false, false, false)]
        [DataRow(false, false, false, true, false, false)]
        [DataRow(false, false, false, false, true, false)]
        [DataRow(false, false, false, false, false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WhenArgumentIsNullExpectException(
            bool nullRecovery,
            bool nullState,
            bool nullProps,
            bool nullDiagnostics,
            bool nullBus,
            bool nullConditions)
        {
            var handler = new BeginGameRoundCommandHandler(
                nullRecovery ? null : _recovery.Object,
                nullDiagnostics ? null : _diagnostics.Object,
                nullState ? null : _gameState.Object,
                nullProps ? null : _properties.Object,
                nullBus ? null : _eventBus.Object,
                nullConditions ? null : _conditions.Object);

            Assert.IsNull(handler);
        }

        [TestMethod]
        public void WhenParamsAreValidExpectSuccess()
        {
            var handler = new BeginGameRoundCommandHandler(
                _recovery.Object,
                _diagnostics.Object,
                _gameState.Object,
                _properties.Object,
                _eventBus.Object,
                _conditions.Object);

            Assert.IsNotNull(handler);
        }

        [TestMethod]
        public void WhenHandleWithSuccessfulIntegrityCheckExpectStart()
        {
            const long denom = 1L;

            _properties.Setup(m => m.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>())).Returns(denom);

            var handler = new BeginGameRoundCommandHandler(
                _recovery.Object,
                _diagnostics.Object,
                _gameState.Object,
                _properties.Object,
                _eventBus.Object,
                _conditions.Object);

            handler.Handle(new BeginGameRound(denom));
        }
    }
}