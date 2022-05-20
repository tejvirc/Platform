namespace Aristocrat.Monaco.Bingo.Commands.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Commands;
    using Common.Storage;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Services.Reporting;

    [TestClass]
    public class BingoGameCompletedCommandHandlerTests
    {
        private readonly Mock<IGameHistoryReportHandler> _reportHandler = new(MockBehavior.Default);

        private BingoGameCompletedCommandHandler _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            _target = new BingoGameCompletedCommandHandler(_reportHandler.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentsTest()
        {
            _ = new BingoGameCompletedCommandHandler(null);
        }

        [TestMethod]
        public async Task NoDescriptionTest()
        {
            var log = new Mock<IGameHistoryLog>();
            var command = new BingoGameEndedCommand("Test Serial", new CentralTransaction(), log.Object);
            await _target.Handle(command, CancellationToken.None);
            _reportHandler.Verify(x => x.AddReportToQueue(It.IsAny<ReportGameOutcomeMessage>()), Times.Never);
        }

        [TestMethod]
        public async Task HasDescriptionTest()
        {
            var log = new Mock<IGameHistoryLog>();
            var transaction = new CentralTransaction { Descriptions = new [] { new BingoGameDescription() } };
            var command = new BingoGameEndedCommand("Test Serial", transaction, log.Object);
            await _target.Handle(command, CancellationToken.None);
            _reportHandler.Verify(x => x.AddReportToQueue(It.IsAny<ReportGameOutcomeMessage>()), Times.Once);
        }
    }
}