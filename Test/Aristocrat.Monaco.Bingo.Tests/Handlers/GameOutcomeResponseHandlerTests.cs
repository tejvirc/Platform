namespace Aristocrat.Monaco.Bingo.Handlers.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Services.GamePlay;

    [TestClass]
    public class GameOutcomeResponseHandlerTests
    {
        private readonly Mock<IBingoGameOutcomeHandler> _outcomeHandler = new(MockBehavior.Default);
        private GameOutcomeResponseHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new GameOutcomeResponseHandler(_outcomeHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullOutComeHandler()
        {
            _target = new GameOutcomeResponseHandler(null);
        }

        [DataRow(false, ResponseCode.Rejected)]
        [DataRow(true, ResponseCode.Ok)]
        [DataTestMethod]
        public async Task HandleTest(bool accepted, ResponseCode expectedCode)
        {
            var bingoDetails = new GameOutcomeBingoDetails(1, Array.Empty<CardPlayed>(), new[] { 23, 41, 55 }, 0);
            var gameDetails = new GameOutcomeGameDetails(1, 123, 1234, 1000, "Test Paytable", 123456);
            var winDetails = new GameOutcomeWinDetails(0, string.Empty, Array.Empty<WinResult>());
            var outcome = new GameOutcome(
                ResponseCode.Ok,
                winDetails,
                gameDetails,
                bingoDetails,
                true,
                false);

            _outcomeHandler.Setup(x => x.ProcessGameOutcome(outcome, It.IsAny<CancellationToken>())).Returns(Task.FromResult(accepted));
            var result = await _target.Handle(outcome, CancellationToken.None);
            Assert.AreEqual(expectedCode, result.ResponseCode);
        }
    }
}