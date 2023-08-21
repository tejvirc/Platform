namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Aristocrat.Monaco.Bingo.Commands;
    using Aristocrat.Monaco.Bingo.Services.GamePlay;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class ClaimWinCommandHandlerTests
    {
        private readonly Mock<IGameOutcomeService> _outcomeService = new Mock<IGameOutcomeService>(MockBehavior.Default);
        private readonly Mock<IBingoGameOutcomeHandler> _outcomeHandler = new Mock<IBingoGameOutcomeHandler>(MockBehavior.Default);

        private ClaimWinCommandHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = CreateTarget();
        }

        [DataRow(true, false)]
        [DataRow(false, true)]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest(bool nullOutcomeService, bool nullOutcomeHandler)
        {
            _target = CreateTarget(nullOutcomeService, nullOutcomeHandler);
        }

        [TestMethod]
        public async Task HandleTest()
        {
            const string serialNumber = "ABC";
            const long gameSerial = 123;
            const uint cardSerial = 1234;
            _outcomeService.Setup(x => x.ClaimWin(It.Is<RequestClaimWinMessage>(m => m.MachineSerial == serialNumber && m.GameSerial == gameSerial && m.CardSerial == cardSerial), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ClaimWinResults(ResponseCode.Ok, true, gameSerial, cardSerial)))
                .Verifiable();
            _outcomeHandler.Setup(x => x.ProcessClaimWin(It.Is<ClaimWinResults>(m => m.ResponseCode == ResponseCode.Ok && m.Accepted && m.GameSerial == gameSerial && m.CardSerial == cardSerial), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Verifiable();
            await _target.Handle(new ClaimWinCommand(serialNumber, gameSerial, cardSerial, 1), CancellationToken.None);
            _outcomeService.Verify();
            _outcomeHandler.Verify();
        }

        private ClaimWinCommandHandler CreateTarget(
            bool nullOutcomeService = false,
            bool nullOutcomeHandler = false)
        {
            return new ClaimWinCommandHandler(
                nullOutcomeService ? null : _outcomeService.Object,
                nullOutcomeHandler ? null : _outcomeHandler.Object);
        }
    }
}