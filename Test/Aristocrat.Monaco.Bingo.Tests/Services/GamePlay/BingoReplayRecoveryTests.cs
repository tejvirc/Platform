namespace Aristocrat.Monaco.Bingo.Tests.Services.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Gaming.Contracts.Bonus;
    using Bingo.GameEndWin;
    using Bingo.Services.GamePlay;
    using Commands;
    using Common.Events;
    using Common.GameOverlay;
    using Common.Storage;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;
    using Test.Common;
    using GameEndWinFactory =
        Common.IBingoStrategyFactory<Aristocrat.Monaco.Bingo.GameEndWin.IGameEndWinStrategy, Common.Storage.Model.GameEndWinStrategy>;

    [TestClass]
    public class BingoReplayRecoveryTests
    {
        private readonly Mock<IEventBus> _eventBus = new();
        private readonly Mock<ICentralProvider> _centralProvider = new();
        private readonly Mock<IPropertiesManager> _properties = new();
        private readonly Mock<IGameHistory> _history = new();
        private readonly Mock<IMessageDisplay> _messages = new();
        private readonly Mock<ICommandHandlerFactory> _commandFactory = new();
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new();
        private readonly Mock<GameEndWinFactory> _gameEndWinFactory = new();
        private readonly Mock<IBonusHandler> _bonusHandler = new();
        private readonly Mock<ITransactionHistory> _transactionHistory = new();
        private readonly Mock<IGamePlayState> _gamePlayState = new();

        private Action<GameLoadedEvent> _gameLoadedConsumer;
        private Action<GamePlayInitiatedEvent> _gamePlayInitiatedConsumer;
        private Action<GameProcessExitedEvent> _gameExitedConsumer;

        private readonly BingoCard _mockCard = new(
            new BingoNumber[,]
            {
                {
                    new(14, BingoNumberState.BallCallInitial), new(40, BingoNumberState.BallCallInitial),
                    new(35, BingoNumberState.BallCallInitial), new(42, BingoNumberState.BallCallInitial),
                    new(75, BingoNumberState.BallCallInitial)
                },
                {
                    new(21, BingoNumberState.BallCallInitial), new(22, BingoNumberState.BallCallInitial),
                    new(20, BingoNumberState.BallCallInitial), new(55, BingoNumberState.BallCallInitial),
                    new(62, BingoNumberState.BallCallInitial)
                },
                {
                    new(70, BingoNumberState.BallCallInitial), new(35, BingoNumberState.BallCallInitial),
                    new(0, BingoNumberState.BallCallInitial), new(5, BingoNumberState.BallCallInitial),
                    new(60, BingoNumberState.BallCallInitial)
                },
                {
                    new(61, BingoNumberState.BallCallInitial), new(11, BingoNumberState.BallCallInitial),
                    new(1, BingoNumberState.BallCallInitial), new(2, BingoNumberState.BallCallInitial),
                    new(3, BingoNumberState.BallCallInitial)
                },
                {
                    new(4, BingoNumberState.BallCallInitial), new(23, BingoNumberState.BallCallInitial),
                    new(24, BingoNumberState.BallCallInitial), new(25, BingoNumberState.BallCallInitial),
                    new(26, BingoNumberState.BallCallInitial)
                }
            },
            123,
            54321,
            54321,
            false);

        private BingoReplayRecovery _target;

        [TestInitialize]
        public void MyTestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Default);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameLoadedEvent>>()))
                .Callback<object, Action<GameLoadedEvent>>((_, handler) => _gameLoadedConsumer = handler);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<GamePlayInitiatedEvent>>()))
                .Callback<object, Action<GamePlayInitiatedEvent>>((_, handler) => _gamePlayInitiatedConsumer = handler);
            _eventBus.Setup(x => x.Subscribe(It.IsAny<object>(), It.IsAny<Action<GameProcessExitedEvent>>()))
                .Callback<object, Action<GameProcessExitedEvent>>((_, handler) => _gameExitedConsumer = handler);
            _target = CreateTarget();
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            MoqServiceManager.RemoveInstance();
            _target?.Dispose();
        }

        [DataRow(true, false, false, false, false, false, false, false, false, DisplayName = "EventBus Null")]
        [DataRow(false, true, false, false, false, false, false, false, false, DisplayName = "CentralProvider Null")]
        [DataRow(false, false, true, false, false, false, false, false, false, DisplayName = "PropertiesManger Null")]
        [DataRow(false, false, false, true, false, false, false, false, false, DisplayName = "GameHistory Null")]
        [DataRow(false, false, false, false, true, false, false, false, false, DisplayName = "MessageHandler Null")]
        [DataRow(false, false, false, false, false, true, false, false, false, DisplayName = "CommandFactory Null")]
        [DataRow(false, false, false, false, false, false, true, false, false, DisplayName = "UnitOfWorkFactory Null")]
        [DataRow(false, false, false, false, false, false, false, true, false, DisplayName = "GameEndWinFactory Null")]
        [DataRow(false, false, false, false, false, false, false, false, true, DisplayName = "BonusHandler Null")]
        [ExpectedException(typeof(ArgumentNullException))]
        [DataTestMethod]
        public void NullConstructorArgumentsTests(
            bool nullEvent,
            bool nullCentralProvider,
            bool nullProperties,
            bool nullHistory,
            bool nullMessages,
            bool nullCommandFactory,
            bool nullUnitOfWOrk,
            bool nullGewFactory,
            bool nullBonusHandler)
        {
            _ = CreateTarget(
                nullEvent,
                nullCentralProvider,
                nullProperties,
                nullHistory,
                nullMessages,
                nullCommandFactory,
                nullUnitOfWOrk,
                nullGewFactory,
                nullBonusHandler);
        }

        [DataRow(false)]
        [DataRow(true)]
        [DataTestMethod]
        public async Task RecoveryAcknowledgedTransactionTest(bool playPatterns)
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoDaubsModel>>()))
                .Returns((BingoDaubsModel)null);

            _gameLoadedConsumer?.Invoke(null);
            var description = new BingoGameDescription
            {
                BallCallNumbers = Enumerable.Repeat(new BingoNumber(123, BingoNumberState.BallCallInitial), 40),
                Cards = new List<BingoCard> { _mockCard },
                Patterns = new List<BingoPattern> { new("Test Pattern", 123, 123, 100, 25, 4, false, 0x40, 1) },
                GameEndWinClaimAccepted = false
            };

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var transactions = new List<CentralTransaction>
            {
                new(1, DateTime.UtcNow, 123, string.Empty, 1, gamePlayInfo)
                {
                    Descriptions = new List<IOutcomeDescription> { description },
                    OutcomeState = OutcomeState.Acknowledged
                }
            };

            _history.Setup(x => x.IsRecoveryNeeded).Returns(!playPatterns);
            _centralProvider.Setup(x => x.Transactions).Returns(transactions);
            await _target.RecoverDisplay(CancellationToken.None);
            foreach (var card in description.Cards)
            {
                _eventBus.Verify(x => x.Publish(It.Is<BingoGameNewCardEvent>(e => e.BingoCard == card)));
            }

            _eventBus.Verify(
                x => x.Publish(
                    It.Is<BingoGamePatternEvent>(
                        e => description.Patterns.SequenceEqual(e.Patterns) && e.StartPatternCycle == playPatterns)));
            _eventBus.Verify(
                x => x.Publish(It.Is<BingoGameBallCallEvent>(e => description.BallCallNumbers.SequenceEqual(e.BallCall.Numbers))));
        }

        [DataRow(false)]
        [DataRow(true)]
        [DataTestMethod]
        public async Task RecoveryDaubStates(bool bingoCardDaubed)
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoDaubsModel>>()))
                .Returns(new BingoDaubsModel { CardIsDaubed = bingoCardDaubed });

            _gameLoadedConsumer?.Invoke(null);
            var description = new BingoGameDescription
            {
                BallCallNumbers = Enumerable.Repeat(new BingoNumber(123, BingoNumberState.BallCallInitial), 40),
                Cards = new List<BingoCard> { _mockCard },
                Patterns = new List<BingoPattern> { new("Test Pattern", 123, 123, 100, 25, 4, false, 0x40, 1) },
                GameEndWinClaimAccepted = false
            };

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var transactions = new List<CentralTransaction>
            {
                new(1, DateTime.UtcNow, 123, string.Empty, 1, gamePlayInfo)
                {
                    Descriptions = new List<IOutcomeDescription> { description },
                    OutcomeState = OutcomeState.Acknowledged
                }
            };

            _history.Setup(x => x.IsRecoveryNeeded).Returns(false);
            _centralProvider.Setup(x => x.Transactions).Returns(transactions);
            await _target.RecoverDisplay(CancellationToken.None);
            foreach (var card in description.Cards)
            {
                _eventBus.Verify(x => x.Publish(It.Is<BingoGameNewCardEvent>(e => e.BingoCard == card)));
            }

            _eventBus.Verify(
                x => x.Publish(It.Is<BingoGamePatternEvent>(e => description.Patterns.SequenceEqual(e.Patterns))),
                bingoCardDaubed ? Times.Once() : Times.Never());
            _eventBus.Verify(
                x => x.Publish(
                    It.Is<BingoGameBallCallEvent>(
                        e => description.BallCallNumbers.SequenceEqual(e.BallCall.Numbers) &&
                             (bingoCardDaubed && e.Daubs == _mockCard.DaubedBits ||
                              !bingoCardDaubed && e.Daubs == 0))));
        }

        [TestMethod]
        public async Task RecoveryGameLoadedTimeoutTest()
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoDaubsModel>>()))
                .Returns((BingoDaubsModel)null);

            _gameExitedConsumer?.Invoke(null);
            var description = new BingoGameDescription
            {
                BallCallNumbers = Enumerable.Repeat(new BingoNumber(123, BingoNumberState.BallCallInitial), 40),
                Cards = new List<BingoCard> { _mockCard },
                Patterns = new List<BingoPattern> { new("Test Pattern", 123, 123, 100, 25, 4, false, 0x40, 1) },
                GameEndWinClaimAccepted = false
            };

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var transactions = new List<CentralTransaction>
            {
                new(1, DateTime.UtcNow, 123, string.Empty, 1, gamePlayInfo)
                {
                    Descriptions = new List<IOutcomeDescription> { description },
                    OutcomeState = OutcomeState.Acknowledged
                }
            };

            _history.Setup(x => x.IsRecoveryNeeded).Returns(false);
            _centralProvider.Setup(x => x.Transactions).Returns(transactions);
            await _target.RecoverDisplay(CancellationToken.None);
            foreach (var card in description.Cards)
            {
                _eventBus.Verify(x => x.Publish(It.Is<BingoGameNewCardEvent>(e => e.BingoCard == card)));
            }

            _eventBus.Setup(
                x => x.Publish(
                    It.Is<BingoGamePatternEvent>(
                        e => description.Patterns.SequenceEqual(e.Patterns) && e.StartPatternCycle)));
            _eventBus.Setup(
                x => x.Publish(It.Is<BingoGameBallCallEvent>(e => description.BallCallNumbers.SequenceEqual(e.BallCall.Numbers))));
        }

        [TestMethod]
        public async Task RecoveryUnacknowledgedTransactionTest()
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoDaubsModel>>()))
                .Returns((BingoDaubsModel)null);

            _gameLoadedConsumer?.Invoke(null);
            var description = new BingoGameDescription
            {
                BallCallNumbers = Enumerable.Repeat(new BingoNumber(123, BingoNumberState.BallCallInitial), 40),
                Cards = new List<BingoCard> { _mockCard },
                Patterns = new List<BingoPattern> { new("Test Pattern", 123, 123, 100, 25, 4, true, 0x40, 1) },
                GameEndWinClaimAccepted = false
            };

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var transactions = new List<CentralTransaction>
            {
                new(1, DateTime.UtcNow, 123, string.Empty, 1, gamePlayInfo)
                {
                    Descriptions = new List<IOutcomeDescription> { description },
                    OutcomeState = OutcomeState.Committed
                }
            };

            _history.Setup(x => x.IsRecoveryNeeded).Returns(true);
            _centralProvider.Setup(x => x.Transactions).Returns(transactions);
            await _target.RecoverDisplay(CancellationToken.None);
            foreach (var card in description.Cards)
            {
                _eventBus.Verify(x => x.Publish(It.Is<BingoGameNewCardEvent>(e => e.BingoCard == card)));
            }

            _eventBus.Setup(
                x => x.Publish(It.Is<BingoGamePatternEvent>(e => description.Patterns.SequenceEqual(e.Patterns))));
            _eventBus.Setup(
                x => x.Publish(It.Is<BingoGameBallCallEvent>(e => description.BallCallNumbers.SequenceEqual(e.BallCall.Numbers))));
        }

        [TestMethod]
        public async Task RecoveryHasGameEndWinMessage()
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoDaubsModel>>()))
                .Returns((BingoDaubsModel)null);

            const long cdsId = 5;
            const long gameId = 1235;
            _gameLoadedConsumer?.Invoke(null);
            var description = new BingoGameDescription
            {
                BallCallNumbers = Enumerable.Repeat(new BingoNumber(123, BingoNumberState.BallCallInitial), 40),
                Cards = new List<BingoCard> { _mockCard },
                Patterns = new List<BingoPattern> { new("Test Pattern", 123, 123, 100, 25, 4, true, 0x40, 1) },
                GameEndWinClaimAccepted = true
            };

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var transactions = new List<CentralTransaction>
            {
                new(1, DateTime.UtcNow, 123, string.Empty, 1, gamePlayInfo)
                {
                    Descriptions = new List<IOutcomeDescription> { description },
                    OutcomeState = OutcomeState.Committed,
                    TransactionId = cdsId,
                    AssociatedTransactions = new List<long> { gameId },
                }
            };

            var bonusTransactions = new List<BonusTransaction>
            {
                new(1, DateTime.UtcNow, "test", 123, 1000, 345, 3, 1, PayMethod.Credit)
                {
                    Mode = BonusMode.GameWin,
                    AssociatedTransactions = new List<long> { 1235}
                }
            };

            var log = new Mock<IGameHistoryLog>();
            log.Setup(x => x.GameWinBonus).Returns(500);
            log.Setup(x => x.TransactionId).Returns(gameId);
            _history.Setup(x => x.GetGameHistory()).Returns(new List<IGameHistoryLog> { log.Object });
            _history.Setup(x => x.IsRecoveryNeeded).Returns(true);
            _centralProvider.Setup(x => x.Transactions).Returns(transactions);
            _bonusHandler.Setup(x => x.Transactions).Returns(bonusTransactions);
            await _target.RecoverDisplay(CancellationToken.None);
            foreach (var card in description.Cards)
            {
                _eventBus.Verify(x => x.Publish(It.Is<BingoGameNewCardEvent>(e => e.BingoCard == card)));
            }

            _eventBus.Setup(
                x => x.Publish(It.Is<BingoGamePatternEvent>(e => description.Patterns.SequenceEqual(e.Patterns))));
            _eventBus.Setup(
                x => x.Publish(It.Is<BingoGameBallCallEvent>(e => description.BallCallNumbers.SequenceEqual(e.BallCall.Numbers))));
            _messages.Verify(x => x.DisplayMessage(It.IsAny<DisplayableMessage>()), Times.Once);
        }

        [DataTestMethod]
        [DataRow(true, false, true, true, OutcomeState.Committed, GameEndWinStrategy.OneCentPerPlayer, true, true, true, DisplayName = "Recover GEW when awarded")]
        [DataRow(true, true, true, true, OutcomeState.Committed, GameEndWinStrategy.OneCentPerPlayer, true, true, false, DisplayName = "Recover after GEW was awarded")]
        [DataRow(true, false, true, true, OutcomeState.Committed, GameEndWinStrategy.OneCentPerPlayer, false, true, true, DisplayName = "Recover GEW when wasn't awarded")]
        [DataRow(false, false, true, true, OutcomeState.Committed, GameEndWinStrategy.OneCentPerPlayer, true, true, false, DisplayName = "Recover when there is GEW pattern")]
        [DataRow(true, false, true, true, OutcomeState.Acknowledged, GameEndWinStrategy.OneCentPerPlayer, true, false, false, DisplayName = "Recover after transaction was acknowledged")]
        [DataRow(true, false, true, false, OutcomeState.Committed, GameEndWinStrategy.OneCentPerPlayer, true, false, false, DisplayName = "Recover when there is no central transaction to recover")]
        [DataRow(true, false, false, true, OutcomeState.Committed, GameEndWinStrategy.OneCentPerPlayer, true, true, false, DisplayName = "Recover when there is no game history to recover")]
        public async Task RecoverGamePlayTest(
            bool hasGewPattern,
            bool gewAlreadyClaimed,
            bool hasActiveGame,
            bool hasTransaction,
            OutcomeState outcomeState,
            GameEndWinStrategy gewStrategy,
            bool gewResult,
            bool handledGameEndCommand,
            bool transactionUpdate)
        {
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoDaubsModel>>()))
                .Returns((BingoDaubsModel)null);

            const long gameId = 1234;
            const long cdsTransactionId = 5;
            const string machineId = "ABC123";
            var description = new BingoGameDescription
            {
                BallCallNumbers = Enumerable.Repeat(new BingoNumber(123, BingoNumberState.BallCallInitial), 40),
                Cards = new List<BingoCard> { _mockCard },
                Patterns = new List<BingoPattern> { new("Test Pattern", 123, 123, 100, 25, 4, hasGewPattern, 0x40, 1) },
                GameEndWinClaimAccepted = gewAlreadyClaimed
            };

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var transaction = new CentralTransaction(1, DateTime.UtcNow, 123, string.Empty, 1, gamePlayInfo)
            {
                Descriptions = new List<IOutcomeDescription> { description },
                AssociatedTransactions = new List<long> { gameId },
                OutcomeState = outcomeState,
                TransactionId = cdsTransactionId
            };

            var log = new Mock<IGameHistoryLog>();
            log.Setup(x => x.TransactionId).Returns(hasActiveGame ? gameId : 0);
            var history = new List<IGameHistoryLog> { log.Object };
            _history.Setup(h => h.GetGameHistory()).Returns(history);
            var transactions = new List<CentralTransaction> { transaction };
            _properties.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, It.IsAny<string>()))
                .Returns(machineId);
            _commandFactory
                .Setup(
                    x => x.Execute(
                        It.Is<BingoGameEndedCommand>(
                            b => b.MachineSerial == machineId && b.Transaction == transaction && b.Log == log.Object),
                        It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            _centralProvider.Setup(x => x.Transactions)
                .Returns(hasTransaction ? transactions : Enumerable.Empty<CentralTransaction>());

            var gewStrategyHandler = new Mock<IGameEndWinStrategy>();
            gewStrategyHandler.Setup(x => x.Recover(gameId, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(gewResult));
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, GameEndWinStrategy?>>()))
                .Returns(gewStrategy);
            _gameEndWinFactory.Setup(x => x.Create(gewStrategy)).Returns(gewStrategyHandler.Object);
            await _target.RecoverGamePlay(CancellationToken.None);
            gewStrategyHandler.Verify(
                x => x.Recover(gameId, It.IsAny<CancellationToken>()),
                transactionUpdate ? Times.Once() : Times.Never());
            _centralProvider.Verify(
                x => x.UpdateOutcomeDescription(
                    cdsTransactionId,
                    It.Is<IEnumerable<IOutcomeDescription>>(d => (d.FirstOrDefault() as BingoGameDescription).GameEndWinClaimAccepted == gewResult)),
                transactionUpdate ? Times.Once() : Times.Never());
            _commandFactory.Verify(
                x => x.Execute(
                    It.Is<BingoGameEndedCommand>(
                        b => b.MachineSerial == machineId && b.Transaction == transaction),
                    It.IsAny<CancellationToken>()),
                handledGameEndCommand ? Times.Once() : Times.Never());
        }

        [DataTestMethod]
        [DataRow(false, DisplayName = "Initial Replay Test")]
        [DataRow(true, DisplayName = "Finalized Replay Test")]
        public async Task ReplayBingoTest(bool isFinalized)
        {
            _gameLoadedConsumer?.Invoke(null);
            const long gameHistoryTransactionId = 1234;
            const int joinedBallIndex = 10;
            var description = new BingoGameDescription
            {
                BallCallNumbers = Enumerable.Repeat(new BingoNumber(123, BingoNumberState.BallCallInitial), 40),
                Cards = new List<BingoCard> { _mockCard },
                Patterns = new List<BingoPattern> { new("Test Pattern", 123, 123, 100, 25, 4, true, 0x40, 1) },
                GameEndWinClaimAccepted = false,
                JoinBallIndex = joinedBallIndex
            };

            var joinedBalls = description.BallCallNumbers.Take(joinedBallIndex);

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var transactions = new List<CentralTransaction>
            {
                new(1, DateTime.UtcNow, 123, string.Empty, 1, gamePlayInfo)
                {
                    Descriptions = new List<IOutcomeDescription> { description },
                    OutcomeState = OutcomeState.Committed,
                    AssociatedTransactions = new List<long> { gameHistoryTransactionId }
                }
            };

            var history = new Mock<IGameHistoryLog>(MockBehavior.Default);
            history.Setup(x => x.TransactionId).Returns(gameHistoryTransactionId);
            _centralProvider.Setup(x => x.Transactions).Returns(transactions);
            await _target.Replay(history.Object, isFinalized, CancellationToken.None);
            foreach (var card in description.Cards)
            {
                _eventBus.Verify(
                    x => x.Publish(It.Is<BingoGameNewCardEvent>(e => e.BingoCard == card)),
                    isFinalized ? Times.Never() : Times.Once());
            }

            _eventBus.Verify(
                x => x.Publish(It.Is<BingoGamePatternEvent>(e => !e.StartPatternCycle && description.Patterns.SequenceEqual(e.Patterns))),
                isFinalized ? Times.Never() : Times.Once());
            _eventBus.Verify(
                x => x.Publish(
                    It.Is<BingoGameBallCallEvent>(
                        e => isFinalized
                            ? description.BallCallNumbers.SequenceEqual(e.BallCall.Numbers)
                            : joinedBalls.Take(description.JoinBallIndex).SequenceEqual(e.BallCall.Numbers))));
        }

        private BingoReplayRecovery CreateTarget(
            bool nullEvent = false,
            bool nullCentralProvider = false,
            bool nullProperties = false,
            bool nullHistory = false,
            bool nullMessages = false,
            bool nullCommandFactory = false,
            bool nullUnitOfWOrk = false,
            bool nullGewFactory = false,
            bool nullBonusHandler = false,
            bool nulltransactionHistory = false,
            bool nullGamePlayState = false)
        {
            return new BingoReplayRecovery(
                nullEvent ? null : _eventBus.Object,
                nullCentralProvider ? null : _centralProvider.Object,
                nullProperties ? null : _properties.Object,
                nullHistory ? null : _history.Object,
                nullMessages ? null : _messages.Object,
                nullCommandFactory ? null : _commandFactory.Object,
                nullUnitOfWOrk ? null : _unitOfWorkFactory.Object,
                nullGewFactory ? null : _gameEndWinFactory.Object,
                nullBonusHandler ? null : _bonusHandler.Object,
                nulltransactionHistory ? null : _transactionHistory.Object,
                nullGamePlayState ? null : _gamePlayState.Object);
        }
    }
}