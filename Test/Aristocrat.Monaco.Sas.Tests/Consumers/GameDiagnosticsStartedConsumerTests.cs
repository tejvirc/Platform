namespace Aristocrat.Monaco.Sas.Tests.Consumers
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Sas.Client;
    using Gaming;
    using Gaming.Contracts;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Sas.Consumers;
    using Sas.Exceptions;
    using Test.Common;

    [TestClass]
    public class GameDiagnosticsStartedConsumerTests
    {
        private readonly Mock<ISasExceptionHandler> _exceptionHandler = new Mock<ISasExceptionHandler>(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
        private readonly Mock<IGameHistory> _gameHistory = new Mock<IGameHistory>(MockBehavior.Default);
        private GameDiagnosticsStartedConsumer _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            MoqServiceManager.CreateAndAddService<ISharedConsumer>(MockBehavior.Default);
            _gameHistory.SetupGet(g => g.LogSequence).Returns(0);
            _target = new GameDiagnosticsStartedConsumer(_exceptionHandler.Object, _gameProvider.Object, _gameHistory.Object);
        }

        [TestCleanup]
        public void MyTestCleanUp()
        {
            MoqServiceManager.RemoveInstance();
        }

        [DataRow(true, false, false, DisplayName = "Null ExceptionHandler")]
        [DataRow(false, true, false, DisplayName = "Null GameProvider")]
        [DataRow(false, false, true, DisplayName = "Null GameHistory")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_InvalidParamas_ThrowsException(
            bool nullExceptionHandler,
            bool nullGameProvider,
            bool nullGameHistory)
        {
            _ = CreateRequestPlayCommandHandler(
                nullExceptionHandler,
                nullGameProvider,
                nullGameHistory);
        }

        private GameDiagnosticsStartedConsumer CreateRequestPlayCommandHandler(bool nullExceptionHandler = false,
            bool nullGameProvider = false,
            bool nullGameHistory = false)
        {
            return new GameDiagnosticsStartedConsumer(
                nullExceptionHandler ? null : _exceptionHandler.Object,
                nullGameProvider ? null : _gameProvider.Object,
                nullGameHistory ? null : _gameHistory.Object);
        }

        [TestMethod]
        public void ConsumeTest()
        {
            const int gameId = 2;
            const long latestLogSequence = 110;
            const long logSequence = 100;
            const long denomId = 5;
            const long denom = 1;
            var expectedResults = new GameRecallEntryDisplayedExceptionBuilder(denomId, latestLogSequence - logSequence);
            GameRecallEntryDisplayedExceptionBuilder actual = null;

            var context = new Mock<IDiagnosticContext<IGameHistoryLog>>(MockBehavior.Default);
            var arguments = new Mock<IGameHistoryLog>(MockBehavior.Default);
            arguments.Setup(x => x.LogSequence).Returns(logSequence);
            context.Setup(x => x.Arguments).Returns(arguments.Object);

            _gameProvider.Setup(x => x.GetGame(gameId)).Returns(
                new MockGameInfo
                {
                    Id = gameId,
                    Denominations = new List<IDenomination> { new Denomination { Id = denomId, Value = denom } }
                });
            _exceptionHandler.Setup(x => x.ReportException(It.IsAny<GameRecallEntryDisplayedExceptionBuilder>()))
                .Callback((ISasExceptionCollection x) => actual = x as GameRecallEntryDisplayedExceptionBuilder)
                .Verifiable();
            _gameHistory.SetupGet(g => g.LogSequence).Returns(latestLogSequence);

            _target.Consume(new GameDiagnosticsStartedEvent(gameId, denom, string.Empty, context.Object));

            Assert.IsNotNull(actual);
            CollectionAssert.AreEquivalent(expectedResults, actual);
            _exceptionHandler.Verify();
        }

        [TestMethod]
        public void ConsumeInvalidContextTest()
        {
            const int gameId = 2;
            const long logSequence = 100;
            const long denom = 1;

            var arguments = new Mock<IGameHistoryLog>(MockBehavior.Default);
            arguments.Setup(x => x.LogSequence).Returns(logSequence);
            _target.Consume(new GameDiagnosticsStartedEvent(gameId, denom, string.Empty, null));
            _exceptionHandler.Verify(
                x => x.ReportException(It.IsAny<GameRecallEntryDisplayedExceptionBuilder>()),
                Times.Never);
        }

        [TestMethod]
        public void ConsumeInvalidGameIdTest()
        {
            const int gameId = 2;
            const long logSequence = 100;
            const long denom = 1;

            var context = new Mock<IDiagnosticContext<IGameHistoryLog>>(MockBehavior.Default);
            var arguments = new Mock<IGameHistoryLog>(MockBehavior.Default);
            arguments.Setup(x => x.LogSequence).Returns(logSequence);
            context.Setup(x => x.Arguments).Returns(arguments.Object);
            _gameProvider.Setup(x => x.GetGame(gameId)).Returns(
                new MockGameInfo
                {
                    Id = gameId,
                    Denominations = new List<IDenomination>()
                });

            _target.Consume(new GameDiagnosticsStartedEvent(gameId, denom, string.Empty, context.Object));
            _exceptionHandler.Verify(
                x => x.ReportException(It.IsAny<GameRecallEntryDisplayedExceptionBuilder>()),
                Times.Never);
        }
    }
}