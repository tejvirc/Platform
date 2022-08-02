namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using Contracts;
    using Contracts.Models;
    using Gaming.Commands;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BeginGameRoundResultsCommandHandlerTests
    {
        private readonly Mock<IGameHistory> _gameHistory = new(MockBehavior.Default);
        private readonly Mock<IGameDiagnostics> _gameDiagnostics = new(MockBehavior.Default);

        private BeginGameRoundResultsCommandHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        public void NullConstructorArgumentsTests(
            bool nullGameDiagnostics,
            bool nullGameHistory)
        {
            Assert.ThrowsException<ArgumentNullException>(() => _ = CreateTarget(nullGameDiagnostics, nullGameHistory));
        }

        [DataRow(false, true, DisplayName = "History is updated when diagnostics are not active")]
        [DataRow(true, false, DisplayName = "History is not updated when diagnostics are active")]
        public void HandleTest(bool diagnosticsActive, bool gameHistoryUpdated)
        {
            const int presentationDetails = 123;
            _gameDiagnostics.Setup(x => x.IsActive).Returns(diagnosticsActive);
            _target.Handle(new BeginGameRoundResults(presentationDetails));
            _gameHistory.Verify(
                x => x.LogGameRoundDetails(It.Is<GameRoundDetails>(d => d.PresentationIndex == presentationDetails)),
                gameHistoryUpdated ? Times.Once() : Times.Never());
        }

        private BeginGameRoundResultsCommandHandler CreateTarget(
            bool nullGameDiagnostics = false,
            bool nullGameHistory = false)
        {
            return new BeginGameRoundResultsCommandHandler(
                nullGameDiagnostics ? null : _gameDiagnostics.Object,
                nullGameHistory ? null : _gameHistory.Object);
        }
    }
}