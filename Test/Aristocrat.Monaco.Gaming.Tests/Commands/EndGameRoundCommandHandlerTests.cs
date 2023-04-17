namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Aristocrat.Monaco.Accounting.Contracts.HandCount;
    using Aristocrat.Monaco.Accounting.Contracts.Handpay;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives;
    using Aristocrat.Monaco.Gaming.Progressives;
    using Contracts;
    using Contracts.Barkeeper;
    using Contracts.Meters;
    using Contracts.Session;
    using Gaming.Commands;
    using Gaming.Runtime;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EndGameRoundCommandHandlerTests
    {
        private const int GameId = 1;
        private const long Denomination = 1000L;

        private readonly Mock<IPlayerBank> _bank = new Mock<IPlayerBank>();
        private readonly Mock<IGameMeterManager> _gameMeters = new Mock<IGameMeterManager>();
        private readonly Mock<IGameHistory> _history = new Mock<IGameHistory>();
        private readonly Mock<IMeterManager> _meters = new Mock<IMeterManager>();
        private readonly Mock<IPlayerService> _players = new Mock<IPlayerService>();
        private readonly Mock<IPropertiesManager> _properties = new Mock<IPropertiesManager>();
        private readonly Mock<IGameRecovery> _recovery = new Mock<IGameRecovery>();
        private readonly Mock<IRuntime> _runtime = new Mock<IRuntime>();
        private readonly Mock<IPersistentStorageManager> _storage = new Mock<IPersistentStorageManager>();
        private readonly Mock<IGameProvider> _games = new Mock<IGameProvider>();
        private readonly Mock<ITransactionHistory> _transactionHistory = new Mock<ITransactionHistory>();
        private readonly Mock<IBarkeeperHandler> _barkeeperHandler = new Mock<IBarkeeperHandler>();
        private readonly Mock<IProgressiveGameProvider> _progressiveGame = new Mock<IProgressiveGameProvider>();
        private readonly Mock<IProgressiveLevelProvider> _progressiveLevel = new Mock<IProgressiveLevelProvider>();
        private readonly Mock<IHandCountService> _handCountServiceProvider = new Mock<IHandCountService>();
        private readonly Mock<IGameHistoryLog> _gameHistory = new Mock<IGameHistoryLog>();
        private Mock<IScopedTransaction> _transactionScope;

        [TestInitialize]
        public void TestInitialize()
        {
            _transactionScope = new Mock<IScopedTransaction>();
            _storage.Setup(m => m.ScopedTransaction()).Returns(_transactionScope.Object);

            _games.Setup(m => m.GetGame(1, Denomination)).Returns(Factory_CreateGame(Denomination));

            _properties.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>()))
                .Returns(GameId);
            _properties.Setup(p => p.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>()))
                .Returns(Denomination);

            _history.SetupGet(h => h.CurrentLog).Returns(_gameHistory.Object);
        }

        [DataTestMethod]
        [DataRow(true, false, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null Bank Test")]
        [DataRow(false, true, false, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Nul Persistence Storage Test")]
        [DataRow(false, false, true, false, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null Game History Test")]
        [DataRow(false, false, false, true, false, false, false, false, false, false, false, false, false, false, DisplayName = "Null Runtime Services Test")]
        [DataRow(false, false, false, false, true, false, false, false, false, false, false, false, false, false, DisplayName = "Null Game Provider Test")]
        [DataRow(false, false, false, false, false, true, false, false, false, false, false, false, false, false, DisplayName = "Null Properties Manager Test")]
        [DataRow(false, false, false, false, false, false, true, false, false, false, false, false, false, false, DisplayName = "Null Game Meters Test")]
        [DataRow(false, false, false, false, false, false, false, true, false, false, false, false, false, false, DisplayName = "Null Player Service Test")]
        [DataRow(false, false, false, false, false, false, false, false, true, false, false, false, false, false, DisplayName = "Null Game Recovery Test")]
        [DataRow(false, false, false, false, false, false, false, false, false, true, false, false, false, false, DisplayName = "Null Transaction History Test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, true, false, false, false, DisplayName = "Null Barkeeper Handler Test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, true, false, false, DisplayName = "Null Progressive Game Test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, true, false, DisplayName = "Null Progressive Level Test")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, false, false, false, true, DisplayName = "Null Hand count service provider Test")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorTest(
            bool nullBank,
            bool nullPersistenceStorage,
            bool nullGameHistory,
            bool nullRuntime,
            bool nullGameProvider,
            bool nullProperties,
            bool nullMeters,
            bool nullPlayers,
            bool nullRecovery,
            bool nullTransactionHistory,
            bool nullBarkeeperHandler,
            bool nullProgressiveGame,
            bool nullProgressiveLevel,
            bool nullHandCountServiceProvider)
        {
            CreateCGameEndCommandHandler(
                nullBank,
                nullPersistenceStorage,
                nullGameHistory,
                nullRuntime,
                nullGameProvider,
                nullProperties,
                nullMeters,
                nullPlayers,
                nullRecovery,
                nullTransactionHistory,
                nullBarkeeperHandler,
                nullProgressiveGame,
                nullProgressiveLevel,
                nullHandCountServiceProvider);
        }

        [TestMethod]
        public void WhenHandleExpectWithInvalidStorageSuccess()
        {
            _properties.Setup(p => p.GetProperty(GamingConstants.GameEndCashOutStrategy, It.IsAny<CashOutStrategy>()))
                .Returns(CashOutStrategy.None);
            _properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(0L);
            _properties.Setup(p => p.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);
            _meters.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(new Mock<IMeter>().Object);
            _gameMeters.Setup(m => m.GetMeter(GameId, Denomination, It.IsAny<string>()))
                .Returns(new Mock<IMeter>().Object);
            _gameMeters.Setup(m => m.GetMeter(GameId, It.IsAny<string>())).Returns(new Mock<IMeter>().Object);

            _storage.Setup(s => s.VerifyIntegrity(true)).Returns(false);

            var handler = CreateCGameEndCommandHandler();

            handler.Handle(new GameEnded());

            _history.Verify(m => m.EndGame());
            _transactionScope.Verify(m => m.Complete());
            _handCountServiceProvider.Verify(m => m.CheckIfBelowResetThreshold());
        }

        [TestMethod]
        public void WhenHandleWithForceCashoutAndUnderLimitExpectNoCashout()
        {
            const long balance = 1L;
            const long limit = 100L;

            _bank.SetupGet(b => b.Balance).Returns(balance);

            _properties.Setup(p => p.GetProperty(GamingConstants.GameEndCashOutStrategy, It.IsAny<CashOutStrategy>()))
                .Returns(CashOutStrategy.Partial);
            _properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(limit);
            _properties.Setup(p => p.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);
            _meters.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(new Mock<IMeter>().Object);
            _gameMeters.Setup(m => m.GetMeter(GameId, Denomination, It.IsAny<string>()))
                .Returns(new Mock<IMeter>().Object);
            _gameMeters.Setup(m => m.GetMeter(GameId, It.IsAny<string>())).Returns(new Mock<IMeter>().Object);

            _storage.Setup(s => s.VerifyIntegrity(true)).Returns(true);

            var handler = CreateCGameEndCommandHandler();

            handler.Handle(new GameEnded());

            _history.Verify(m => m.EndGame());
            _transactionScope.Verify(m => m.Complete());
        }

        [TestMethod]
        public void WhenHandleWithForceFullCashoutAndUnderLimitExpectNoCashout()
        {
            const long balance = 1L;
            const long limit = 100L;

            _bank.SetupGet(b => b.Balance).Returns(balance);

            _properties.Setup(p => p.GetProperty(GamingConstants.GameEndCashOutStrategy, It.IsAny<CashOutStrategy>()))
                .Returns(CashOutStrategy.Full);
            _properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(limit);
            _properties.Setup(p => p.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);
            _meters.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(new Mock<IMeter>().Object);
            _gameMeters.Setup(m => m.GetMeter(GameId, Denomination, It.IsAny<string>()))
                .Returns(new Mock<IMeter>().Object);
            _gameMeters.Setup(m => m.GetMeter(GameId, It.IsAny<string>())).Returns(new Mock<IMeter>().Object);

            _storage.Setup(s => s.VerifyIntegrity(true)).Returns(true);

            var handler = CreateCGameEndCommandHandler();

            handler.Handle(new GameEnded());

            _history.Verify(m => m.EndGame());
            _transactionScope.Verify(m => m.Complete());
        }

        [TestMethod]
        public void WhenHandleLinkedProgressiveWin_IncrementTotalPaidLinkedProgWonAmtMeter()
        {
            _properties.Setup(p => p.GetProperty(GamingConstants.GameEndCashOutStrategy, It.IsAny<CashOutStrategy>()))
            .Returns(CashOutStrategy.None);
            _properties.Setup(p => p.GetProperty(AccountingConstants.MaxCreditMeter, It.IsAny<long>()))
                .Returns(0L);
            _properties.Setup(p => p.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);
            _gameHistory.SetupGet(g => g.Result).Returns(GameResult.Won);
            var dummyMeter = new Mock<IMeter>();
            _gameMeters.Setup(m => m.GetMeter(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>())).Returns(dummyMeter.Object);
            _gameMeters.Setup(m => m.GetMeter(It.IsAny<string>())).Returns(dummyMeter.Object);
            var winAmount = 5000;
            var deviceId = 1;
            var packName = "UnitTestPackage";
            var meter = new Mock<IMeter>();
            meter.Setup(m => m.Increment(winAmount)).Verifiable();
            _gameHistory.SetupGet(g => g.Jackpots).Returns(new List<JackpotInfo>{ new JackpotInfo { PackName = packName, WinAmount = winAmount, DeviceId = deviceId } } );
            var progressiveLevel = new ProgressiveLevel();
            progressiveLevel.ProgressivePackName= packName ;
            progressiveLevel.DeviceId = deviceId;
            progressiveLevel.LevelType = ProgressiveLevelType.LP;
            var readOnlyCollection = new List<ProgressiveLevel> { progressiveLevel };

            _progressiveLevel.Setup(p => p.GetProgressiveLevels(packName, GameId, Denomination, It.IsAny<long>())).Returns(readOnlyCollection).Verifiable();
            _gameMeters.Setup(m => m.GetMeter(GameId, Denomination, GamingMeters.TotalPaidLinkedProgWonAmt)).Returns(meter.Object).Verifiable();
            _transactionHistory.Setup(t => t.RecallTransactions<HandpayTransaction>()).Returns(new List<HandpayTransaction>());
            var handler = CreateCGameEndCommandHandler();

            handler.Handle(new GameEnded());

            _progressiveLevel.Verify();
            _gameMeters.Verify();
            meter.Verify();
        }

        private static (IGameDetail game, IDenomination denom) Factory_CreateGame(long denomination)
        {
            var game = new Mock<IGameDetail>();

            game.Setup(h => h.Id).Returns(1);

            var mockDenom = new Mock<IDenomination>();

            mockDenom.Setup(m => m.Value).Returns(denomination);
            mockDenom.Setup(h => h.SecondaryAllowed).Returns(false);
            mockDenom.Setup(h => h.LetItRideAllowed).Returns(false);

            return (game.Object, mockDenom.Object);
        }

        private GameEndedCommandHandler CreateCGameEndCommandHandler(
            bool nullBank = false,
            bool nullPersistenceStorage = false,
            bool nullGameHistory = false,
            bool nullRuntime = false,
            bool nullGameProvider = false,
            bool nullProperties = false,
            bool nullMeters = false,
            bool nullPlayers = false,
            bool nullRecovery = false,
            bool nullTransactionHistory = false,
            bool nullBarkeeperHandler = false,
            bool nullProgressiveGame = false,
            bool nullProgressiveLevel = false,
            bool nullHandCountServiceProvider = false)
        {
            return new GameEndedCommandHandler(
                nullBank ? null : _bank.Object,
                nullPersistenceStorage ? null : _storage.Object,
                nullGameHistory ? null : _history.Object,
                nullRuntime ? null : _runtime.Object,
                nullGameProvider ? null : _games.Object,
                nullProperties ? null : _properties.Object,
                nullMeters ? null : _gameMeters.Object,
                nullPlayers ? null : _players.Object,
                nullRecovery ? null : _recovery.Object,
                nullTransactionHistory ? null : _transactionHistory.Object,
                nullBarkeeperHandler ? null : _barkeeperHandler.Object,
                nullProgressiveGame ? null : _progressiveGame.Object,
                nullProgressiveLevel ? null : _progressiveLevel.Object,
                nullHandCountServiceProvider ? null : _handCountServiceProvider.Object
                );
        }
    }
}