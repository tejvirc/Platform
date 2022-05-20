namespace Aristocrat.Monaco.Bingo.Handlers.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Aristocrat.Monaco.Bingo.Handlers;
    using Aristocrat.Monaco.Bingo.Services.GamePlay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class GameOutcomeResponseHandlerTests
    {
        private readonly Mock<IBingoGameOutcomeHandler> _outcomeHandler = new Mock<IBingoGameOutcomeHandler>(MockBehavior.Default);
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
            var outcome = new GameOutcome(
                ResponseCode.Ok,
                "ABC",
                0,
                string.Empty,
                123,
                1,
                1,
                1000,
                123,
                "OK",
                true,
                "Test Paytable",
                0,
                new List<CardPlayed>(),
                new List<int> { 23, 41, 55 },
                new List<WinResult>(),
                false);

            _outcomeHandler.Setup(x => x.ProcessGameOutcome(outcome, It.IsAny<CancellationToken>())).Returns(Task.FromResult(accepted));
            var result = await _target.Handle(outcome, CancellationToken.None);
            Assert.AreEqual(expectedCode, result.ResponseCode);
        }
    }
}