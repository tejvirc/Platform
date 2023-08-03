namespace Aristocrat.Monaco.Bingo.Handlers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Services.GamePlay;

    [TestClass]
    public class GameOutcomesResponseHandlerTests
    {
        private readonly Mock<IBingoGameOutcomeHandler> _outcomeHandler = new(MockBehavior.Default);
        private GameOutcomesResponseHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new GameOutcomesResponseHandler(_outcomeHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullOutComeHandler()
        {
            _target = new GameOutcomesResponseHandler(null);
        }

        [DataRow(false, ResponseCode.Rejected)]
        [DataRow(true, ResponseCode.Ok)]
        [DataTestMethod]
        public async Task HandleTest(bool accepted, ResponseCode expectedCode)
        {
            var bingoDetails = new GameOutcomeBingoDetails(1, Array.Empty<CardPlayed>(), new[] { 23, 41, 55 }, 0);
            var gameDetails = new GameOutcomeGameDetails("1", 123, 1234, 1000, "Test Paytable", 123456);
            var winDetails = new GameOutcomeWinDetails(0, string.Empty, Array.Empty<WinResult>());
            var outcome = new GameOutcome(
                ResponseCode.Ok,
                winDetails,
                gameDetails,
                bingoDetails,
                true,
                false);
            var outcomes = new GameOutcomes(ResponseCode.Ok, new List<GameOutcome> { outcome });
            _outcomeHandler.Setup(x => x.ProcessGameOutcomes(outcomes, It.IsAny<CancellationToken>())).Returns(Task.FromResult(accepted));
            var result = await _target.Handle(outcomes, CancellationToken.None);
            Assert.AreEqual(expectedCode, result.ResponseCode);
        }
    }
}
