namespace Aristocrat.Monaco.Gaming.VideoLottery.Tests
{
    // These were dependent on a library that shouldn't have been added to the platform.  They'll need to be updated

    /*
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Accounting.Contracts;
    using Accounting.Contracts.Handpay;
    using Accounting.Contracts.TransferOut;
    using Accounting.Contracts.Wat;
    using Application.Contracts;
    using Contracts.Jackpot;
    using Contracts;
    using FluentAssertions;
    using Hardware.Contracts.Persistence;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GameHistoryTests
    {
        private IPersistenceProvider _persistenceProvider;
        private IPersistentBlock _persistentBlock;
        private IBank _bank;
        private ICurrencyInContainer _currencyHandler;
        private IIdProvider _idProvider;
        private IPropertiesManager _properties;
        private IGameDiagnostics _replay;
        private ISystemDisableManager _systemDisableManager;

        private const int MaxLogsCount = 10;
        private const string GameHistoryKey = @"GameHistory";

        [TestInitialize]
        public void Initialize()
        {
            _persistenceProvider = A.Fake<IPersistenceProvider>();
            _persistentBlock = A.Fake<IPersistentBlock>();
            _bank = A.Fake<IBank>();
            _currencyHandler = A.Fake<ICurrencyInContainer>();
            _idProvider = A.Fake<IIdProvider>();
            _properties = A.Fake<IPropertiesManager>();
            _replay = A.Fake<IGameDiagnostics>();
            _systemDisableManager = A.Fake<ISystemDisableManager>();

            A.CallTo(() => _bank.QueryBalance()).Returns(1000);
            A.CallTo(() => _properties.GetProperty(
                GamingConstants.MaxGameHistory, GamingConstants.DefaultMaxGameHistory)).Returns(MaxLogsCount);
            A.CallTo(() => _properties.GetProperty(
                GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);
            A.CallTo(() => _persistenceProvider.GetOrCreateBlock(
                GameHistoryKey, PersistenceLevel.Critical)).Returns(_persistentBlock);
        }

        [TestMethod]
        public void CTor_ShouldFail_WhenPassingNullValues()
        {
            GameHistory gameHistory = null;

            var exception = Xunit.Assert.Throws<ArgumentNullException>(() => gameHistory = new GameHistory(null, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider));
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: properties");
            gameHistory.Should().BeNull();

            exception = Xunit.Assert.Throws<ArgumentNullException>(() => gameHistory = new GameHistory(_properties, null, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider));
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: bank");
            gameHistory.Should().BeNull();

            exception = Xunit.Assert.Throws<ArgumentNullException>(() => gameHistory = new GameHistory(_properties, _bank, null, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider));
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: gameDiagnostics");
            gameHistory.Should().BeNull();

            exception = Xunit.Assert.Throws<ArgumentNullException>(() => gameHistory = new GameHistory(_properties, _bank, _replay, null, _systemDisableManager, _currencyHandler, _persistenceProvider));
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: idProvider");
            gameHistory.Should().BeNull();

            exception = Xunit.Assert.Throws<ArgumentNullException>(() => gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, null, _currencyHandler, _persistenceProvider));
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: systemDisable");
            gameHistory.Should().BeNull();

            exception = Xunit.Assert.Throws<ArgumentNullException>(() => gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, null, _persistenceProvider));
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: currencyHandler");
            gameHistory.Should().BeNull();

            exception = Xunit.Assert.Throws<ArgumentNullException>(() => gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, null));
            exception.Message.Should().Be("Value cannot be null.\r\nParameter name: persistenceProvider");
            gameHistory.Should().BeNull();
        }

        [TestMethod]
        public void CTor_ShouldLoadHistory_WhenCreatedHistoryDoesNotExist()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                GameHistoryLog gameHistoryLog = null;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(false);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            var gameHistoryLogs = gameHistory.GetGameHistory();

            gameHistoryLogs.Count().Should().Be(0);

            gameHistory.CurrentLog.Should().BeNull();

            A.CallTo(() => _systemDisableManager.Disable(A<Guid>._, A<SystemDisablePriority>._, A<string>._, null)).MustNotHaveHappened();
        }

        [TestMethod]
        public void CTor_ShouldLoadHistory_WhenCreatedHistoryExists()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            var gameHistoryLogs = gameHistory.GetGameHistory();

            gameHistoryLogs.Count().Should().Be(MaxLogsCount);

            gameHistory.CurrentLog.TransactionId.Should().Be(MaxLogsCount - 1);

            A.CallTo(() => _systemDisableManager.Disable(A<Guid>._, A<SystemDisablePriority>._, A<string>._, null)).MustNotHaveHappened();
        }

        [TestMethod]
        public void CTor_ShouldLoadHistoryAndDisableSystem_WhenCreatedHistoryExistsAndIsGameFatalError()
        {
            CTor_ShouldLoadHistoryAndDisableSystem_WhenCreatedHistoryExistsAndIsGameFatalError(ErrorCode.LegitimacyLimit);
            CTor_ShouldLoadHistoryAndDisableSystem_WhenCreatedHistoryExistsAndIsGameFatalError(ErrorCode.LiabilityLimit);
        }

        private void CTor_ShouldLoadHistoryAndDisableSystem_WhenCreatedHistoryExistsAndIsGameFatalError(ErrorCode errorCode)
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;

                if (i == MaxLogsCount - 1)
                {
                    gameHistoryLog.PlayState = PlayState.FatalError;
                    gameHistoryLog.ErrorCode = errorCode;
                }

                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            var gameHistoryLogs = gameHistory.GetGameHistory();

            gameHistoryLogs.Count().Should().Be(MaxLogsCount);

            gameHistory.CurrentLog.TransactionId.Should().Be(MaxLogsCount - 1);

            var disableReason = ErrorCode.LiabilityLimit == errorCode
                ? Properties.Resources.LiabilityCheckFailed
                : Properties.Resources.LegitimacyCheckFailed;

            A.CallTo(() => _systemDisableManager.Disable(GamingConstants.FatalGameErrorGuid, SystemDisablePriority.Immediate, disableReason, null)).MustHaveHappened();
        }

        [TestMethod]
        public void End_ShouldReturnWithoutDoingAnything_WhenIsReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.PlayState = PlayState.GameEnded;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);
            //_properties.Setup(mock => mock.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
            //    .Returns(false);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.End();
            gameHistory.CurrentLog.PlayState.Should().NotBe(PlayState.Idle);
        }

        [TestMethod]
        public void IncrementUncommittedWin_ShouldUpdateIncrementUncommittedWin_WhenIsNotReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);
            //_properties.Setup(mock => mock.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
            //    .Returns(false);

            A.CallTo(() => _replay.IsActive).Returns(false);
            var uncommittedWin = gameHistory.CurrentLog.UncommittedWin;
            gameHistory.IncrementUncommittedWin(200);
            gameHistory.CurrentLog.UncommittedWin.Should().Be(200);
        }

        [TestMethod]
        public void IncrementUncommittedWin_ShouldReturnWithoutDoingAnything_WhenIsReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);
            var uncommittedWin = gameHistory.CurrentLog.UncommittedWin;
            gameHistory.IncrementUncommittedWin(200);
            gameHistory.CurrentLog.UncommittedWin.Should().Be(uncommittedWin);
        }

        //[TestMethod]
        //public void GetByTransactionId_ShouldReturnLog_WhenExists()
        //{
        //    for (int i = 0; i < MaxLogsCount; i++)
        //    {
        //        var gameHistoryLog = A.Fake<GameHistoryLog>();
        //        gameHistoryLog.TransactionId = 10 * i;
        //        gameHistoryLog.InitialWin = 10 * i + 3;
        //        gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
        //        A.CallTo(() => _persistentBlock.GetValue(GameHistoryKey, i, out gameHistoryLog)).Returns(true);
        //    }

        //    var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

        //    A.CallTo(() => _replay.IsReplaying).Returns(true);

        //    var response = gameHistory.GetByTransactionId(60);

        //    response.TransactionId.Should().Be(60);
        //    response.InitialWin.Should().Be(63);
        //}

        //[TestMethod]
        //public void GetByTransactionId_ShouldReturnNull_WhenNotExist()
        //{
        //    for (int i = 0; i < MaxLogsCount; i++)
        //    {
        //        var gameHistoryLog = A.Fake<GameHistoryLog>();
        //        gameHistoryLog.TransactionId = i;
        //        gameHistoryLog.InitialWin = 10 * i;
        //        gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
        //        A.CallTo(() => _persistentBlock.GetValue(GameHistoryKey, i, out gameHistoryLog)).Returns(true);
        //    }

        //    var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

        //    A.CallTo(() => _replay.IsReplaying).Returns(true);

        //    var response = gameHistory.GetByTransactionId(MaxLogsCount + 4);

        //    response.Should().BeNull();
        //}

        [TestMethod]
        public void GetByIndex_ShouldReturnNull_WhenNotExist()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 10 * i;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);
            //_properties.Setup(mock => mock.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
            //    .Returns(false);

            A.CallTo(() => _replay.IsActive).Returns(true);

            var response = gameHistory.GetByIndex(MaxLogsCount + 4);

            response.Should().BeNull();
        }

        [TestMethod]
        public void GetByIndex_ShouldReturnLog_WhenExist()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 10 * i;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            var response = gameHistory.GetByIndex(4);

            response.TransactionId.Should().Be(4);
            response.InitialWin.Should().Be(40);
        }

        [TestMethod]
        public void GetGameHistory_ShouldReturnAllLogs_WhenExist()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 10 * i;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);
            //_properties.Setup(mock => mock.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
            //    .Returns(false);

            A.CallTo(() => _replay.IsActive).Returns(true);

            var allLogs = gameHistory.GetGameHistory();

            allLogs.Count().Should().Be(MaxLogsCount);
        }

        [TestMethod]
        public void GetGameHistory_ShouldReturnZeroLogs_WhenNotExist()
        {
            GameHistoryLog gameHistoryLog = null;
            A.CallTo(() => _persistentBlock.GetValue(GameHistoryKey, A<int>.Ignored, out gameHistoryLog)).Returns(false);

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);
            //_properties.Setup(mock => mock.GetProperty(GamingConstants.MeterFreeGamesIndependently, false))
            //    .Returns(false);

            A.CallTo(() => _replay.IsActive).Returns(true);

            var allLogs = gameHistory.GetGameHistory();

            allLogs.Count().Should().Be(0);
        }

        [TestMethod]
        public void LoadReplay_ShouldReturnRecoveryData_WhenExist()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.RecoveryBlob = Encoding.ASCII.GetBytes($"recoveryData{i}");
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            var success = gameHistory.LoadRecoveryPoint(out var log);
            log.Should().Equal($"recoveryData{MaxLogsCount - 1}");

            success = gameHistory.LoadReplay(6, out log);
            log.Should().Equal($"recoveryData6");
        }

        [TestMethod]
        public void SaveRecoveryPoint_ShouldSave_WhenNotReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.RecoveryBlob = Encoding.ASCII.GetBytes($"recoveryData{i}");
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            gameHistory.SaveRecoveryPoint(data);

            var stringData = Encoding.ASCII.GetString(data);
            var privateObject = new PrivateObject(gameHistory.CurrentLog);

            privateObject.GetFieldOrProperty("RecoveryBlob").Should().NotBe(Encoding.ASCII.GetBytes($"recoveryData{MaxLogsCount}"));
        }

        [TestMethod]
        public void SaveRecoveryPoint_ShouldNotSave_WhenInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.RecoveryBlob = Encoding.ASCII.GetBytes($"recoveryData{i}");
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            var data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            gameHistory.SaveRecoveryPoint(data);

            var stringData = Encoding.ASCII.GetString(data);
            var privateObject = new PrivateObject(gameHistory.CurrentLog);

            privateObject.GetFieldOrProperty("RecoveryBlob").Should().NotBe(Encoding.ASCII.GetBytes($"recoveryData{MaxLogsCount}"));
        }

        [TestMethod]
        public void StartFreeGame_ShouldSetFinalWin_WhenNotInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10,  AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)
                    }
                };

                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.StartFreeGame();
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().FinalWin.Should().Be(0);
        }

        [TestMethod]
        public void StartFreeGame_ShouldAddFreeGame_WhenNotInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 2;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)
                    }
                };
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.StartFreeGame();
            gameHistory.CurrentLog.FreeGames.Count().Should().Be(3);
            gameHistory.CurrentLog.FreeGames.ToArray()[2].StartCredits.Should().Be(1000);
        }

        [TestMethod]
        public void StartFreeGame_ShouldNotSetFinalWinToZero_WhenNotInReplayAndFreeGameIndexSmallerThanLastCommitIndex()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 2;
                gameHistoryLog.LastCommitIndex = 3;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)
                    }
                };
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.StartFreeGame();
            gameHistory.CurrentLog.FreeGames.Count().Should().Be(2);
            gameHistory.CurrentLog.FreeGames.ToArray()[1].StartCredits.Should().Be(400);
        }

        [TestMethod]
        public void FreeGameResults_ShouldAddFinalWin_WhenNotInReplayAndFreeGameResultIsNone()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 1;
                gameHistoryLog.LastCommitIndex = 0;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)
                    }
                };
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.FreeGameResults(250);
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().FinalWin.Should().Be(260); ;
        }

        [TestMethod]
        public void FreeGameResults_ShouldNotAddFinalWin_WhenNotInReplayAndFreeGameResultIsNotNone()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 1;
                gameHistoryLog.LastCommitIndex = 0;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10,AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 1, 2)
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)
                    }
                };
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.FreeGameResults(250);
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().FinalWin.Should().Be(10); ;
        }

        [TestMethod]
        public void CommitWin_ShouldUpdateLastCommitIndex_WhenCalled()
        {

            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.LastCommitIndex = -1;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            gameHistory.CommitWin();
            gameHistory.CurrentLog.LastCommitIndex.Should().Be(0);
        }

        [TestMethod]
        public void EndPrimaryGame_ShouldReturnWithoutDoingAnything_WhenIsReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.EndPrimaryGame(250);
            gameHistory.CurrentLog.InitialWin.Should().Be(120);
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PrimaryGameStarted);
        }

        [TestMethod]
        public void EndPrimaryGame_ShouldReturnWithoutDoingAnything_WhenIsNotReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.EndPrimaryGame(250);
            gameHistory.CurrentLog.InitialWin.Should().Be(250);
            gameHistory.CurrentLog.UncommittedWin.Should().Be(0);
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PrimaryGameEnded);
        }

        [TestMethod]
        public void SecondaryGameChoice_ShouldReturnWithoutDoingAnything_WhenIsReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.SecondaryGameChoice();
            gameHistory.CurrentLog.PlayState.Should().NotBe(PlayState.SecondaryGameChoice);
        }

        [TestMethod]
        public void SecondaryGameChoice_ShouldUpdatePlayState_WhenIsNotReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.SecondaryGameChoice();
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.SecondaryGameChoice);
        }

        [TestMethod]
        public void StartSecondaryGame_ShouldReturnWithoutDoingAnything_WhenIsReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.SecondaryWager = 500;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.StartSecondaryGame(200);

            gameHistory.CurrentLog.SecondaryWager.Should().Be(500);
            gameHistory.CurrentLog.PlayState.Should().NotBe(PlayState.SecondaryGameStarted);
        }

        [TestMethod]
        public void StartSecondaryGame_ShouldUpdatePlayState_WhenIsNotReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.SecondaryWager = 500;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.StartSecondaryGame(200);

            gameHistory.CurrentLog.SecondaryWager.Should().Be(700);
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.SecondaryGameStarted);
        }

        [TestMethod]
        public void EndSecondaryGame_ShouldReturnWithoutDoingNothing_WhenIsReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.SecondaryPlayed = 88;
                gameHistoryLog.SecondaryWin = 400;
                gameHistoryLog.SecondaryWager = 500;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.EndSecondaryGame(200);

            gameHistory.CurrentLog.SecondaryPlayed.Should().Be(88);
            gameHistory.CurrentLog.SecondaryWin.Should().Be(400);
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PrimaryGameStarted);
        }

        [TestMethod]
        public void Results_ShouldReturnWithoutDoingNothing_WhenIsReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.SecondaryPlayed = 88;
                gameHistoryLog.FinalWin = 950;
                gameHistoryLog.SecondaryWin = 400;
                gameHistoryLog.SecondaryWager = 500;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.Results(140);

            gameHistory.CurrentLog.FinalWin.Should().Be(950);
        }

        [TestMethod]
        public void Results_ShouldReturnWithoutDoingNothing_WhenFinalWinIsZero()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.SecondaryPlayed = 88;
                gameHistoryLog.FinalWin = 950;
                gameHistoryLog.SecondaryWin = 400;
                gameHistoryLog.SecondaryWager = 500;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.Results(0);

            gameHistory.CurrentLog.FinalWin.Should().Be(950);
        }

        [TestMethod]
        public void Results_ShouldUpdateFinalWin_WhenFinalWinIsNotZeroAndNotRepaly()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.SecondaryPlayed = 88;
                gameHistoryLog.FinalWin = 950;
                gameHistoryLog.SecondaryWin = 400;
                gameHistoryLog.SecondaryWager = 500;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.Results(30);

            gameHistory.CurrentLog.FinalWin.Should().Be(30);
        }

        [TestMethod]
        public void PayResults_ShouldUpdateFinalWin_WhenNotRepaly()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.PayResults();
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PayGameResults);
        }

        [TestMethod]
        public void PayResults_ShouldReturnWithoutDoingAnything_WhenIsRepaly()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.PayResults();
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PrimaryGameStarted);
        }

        [TestMethod]
        public void AppendGameRoundEventInfo_ShouldReturnWithoutDoingAnything_WhenAppendGameWithEmptyList()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.GameRoundDescriptions = "abcd";
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.AppendGameRoundEventInfo(new List<string> { });
            gameHistory.CurrentLog.GameRoundDescriptions.Should().Be("abcd");
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PrimaryGameStarted);
        }

        [TestMethod]
        public void AppendGameRoundEventInfo_ShouldReturnWithoutDoingAnything_WhenIsRepaly()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.GameRoundDescriptions = "abcd";
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.AppendGameRoundEventInfo(new List<string> { "aa", "bb" });
            gameHistory.CurrentLog.GameRoundDescriptions.Should().Be("abcd");
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PrimaryGameStarted);
        }

        [TestMethod]
        public void AppendGameRoundEventInfo_ShouldUpdateGameRoundDescriptions_WhenIsNotRepaly()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.GameRoundDescriptions = "abcd";
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.AppendGameRoundEventInfo(new List<string> { "aa", "bb" });
            gameHistory.CurrentLog.GameRoundDescriptions.Should().Be(@"abcdaa\nbb");
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PrimaryGameStarted);
        }

        [TestMethod]
        public void AppendJackpotInfo_ShouldAddJackpot_WhenNotInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.Jackpots = new[]
                {

                    new JackpotInfo{TransactionId = 62,LevelId = 15,WinAmount = 10},
                    new JackpotInfo{TransactionId = 64,LevelId = 16,WinAmount = 30},
                    new JackpotInfo{TransactionId = 66,LevelId = 17,WinAmount = 20}
                };
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.AppendJackpotInfo(new JackpotInfo { LevelId = 3, TransactionId = 5, WinAmount = 20 });
            gameHistory.CurrentLog.Jackpots.Count().Should().Be(4);
        }

        [TestMethod]
        public void AppendJackpotInfo_ShouldNotAddJackpot_WhenInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.Jackpots = new[]
                {

                    new JackpotInfo{TransactionId = 62,LevelId = 15,WinAmount = 10},
                    new JackpotInfo{TransactionId = 64,LevelId = 16,WinAmount = 30},
                    new JackpotInfo{TransactionId = 66,LevelId = 17,WinAmount = 20}
                };
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.AppendJackpotInfo(new JackpotInfo { LevelId = 3, TransactionId = 5, WinAmount = 20 });
            gameHistory.CurrentLog.Jackpots.Count().Should().Be(3);
        }

        [TestMethod]
        public void AppendJackpotInfo_ShouldDoNothing_WhenInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.Jackpots = new[]
                {

                    new JackpotInfo{TransactionId = 62,LevelId = 15,WinAmount = 10},
                    new JackpotInfo{TransactionId = 64,LevelId = 16,WinAmount = 30},
                    new JackpotInfo{TransactionId = 66,LevelId = 17,WinAmount = 20}
                };
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.AppendJackpotInfo(new JackpotInfo { LevelId = 3, TransactionId = 5, WinAmount = 20 });
            gameHistory.CurrentLog.Jackpots.Count().Should().Be(3);
        }

        [TestMethod]
        public void AppendJackpotInfo_ShouldUpdateJackpots_WhenNotInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.Jackpots = new[]
                {

                    new JackpotInfo{TransactionId = 62,LevelId = 15,WinAmount = 10},
                    new JackpotInfo{TransactionId = 64,LevelId = 16,WinAmount = 30},
                    new JackpotInfo{TransactionId = 66,LevelId = 17,WinAmount = 20}
                };
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.AppendJackpotInfo(new JackpotInfo { LevelId = 3, TransactionId = 5, WinAmount = 20 });
            gameHistory.CurrentLog.Jackpots.Count().Should().Be(4);
        }

        [TestMethod]
        public void AppendCashOut_ShouldUpdateCashOutInfoAndCallResetCurrencyHandler_WhenSuccesfull()
        {
            var resetAmount = 20;
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.CashOutInfo = new List<CashOutInfo>();
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame{FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue},
                    new FreeGame{FinalWin = 190,AmountOut = 40, StartCredits = 400, EndCredits = 500,StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)}
                };
                gameHistoryLog.Jackpots = new[]
                {
                    new JackpotInfo{TransactionId = 62,LevelId = 15,WinAmount = 10},
                    new JackpotInfo{TransactionId = 64,LevelId = 16,WinAmount = 30},
                    new JackpotInfo{TransactionId = 66,LevelId = 17,WinAmount = 20}
                };
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _currencyHandler.Reset()).Returns(resetAmount);

            var traceId = Guid.NewGuid();
            gameHistory.AppendCashOut(new CashOutInfo { Amount = 155, TraceId = traceId });
            var privateObject = new PrivateObject(gameHistory.CurrentLog);
            var cashOutInfo = privateObject.GetFieldOrProperty("CashOutInfo");
            ((IEnumerable<CashOutInfo>)cashOutInfo).Count().Should().Be(1);
            ((IEnumerable<CashOutInfo>)cashOutInfo).FirstOrDefault().Amount.Should().Be(155);
            ((IEnumerable<CashOutInfo>)cashOutInfo).FirstOrDefault().TraceId.Should().Be(traceId);

            A.CallTo(() => _currencyHandler.Reset()).MustHaveHappened();
        }

        //[TestMethod]
        //public void AppendCashOut_ShouldUpdateAmountOutOnLogOrFreeGameAndAndCallResetCurrencyHandler_WhenPassingTransaction()
        //{
        //    var handpayTransaction = new HandpayTransaction(1, DateTime.Now, 10, 13, 15, HandpayType.CancelCredit, true, Guid.NewGuid());
        //    AppendCashOutTransaction(handpayTransaction, true, 0, 78, 38);
        //    AppendCashOutTransaction(handpayTransaction, false, 38, 40, 38);

        //    var watTransaction = new WatTransaction(3, DateTime.Now, 10, 13, 15, true, "reqId");
        //    AppendCashOutTransaction(watTransaction, true, 0, 78, 38);
        //    AppendCashOutTransaction(watTransaction, false, 38, 40, 38);

        //    var voucherOutTransaction = new VoucherOutTransaction(3, DateTime.Now, 10, AccountType.Cashable, "barcode", 200, 12, "ver");
        //    AppendCashOutTransaction(voucherOutTransaction, true, 0, 50, 10);
        //    AppendCashOutTransaction(voucherOutTransaction, false, 10, 40, 10);
        //}

        //private void AppendCashOutTransaction(ITransaction transaction, bool applyToFreeGame, int expectedLogAmountOut, int expectedFreeGameAmountOut, int expectedTransactionAmount)
        //{
        //    var resetAmount = 20;
        //    for (int i = 0; i < MaxLogsCount; i++)
        //    {
        //        var gameHistoryLog = A.Fake<GameHistoryLog>();
        //        gameHistoryLog.TransactionId = i;
        //        gameHistoryLog.Transactions = new List<TransactionInfo> { new TransactionInfo { Amount = 202, Time = new DateTime(2002, 3, 4), TransactionType = typeof(int) } };
        //        gameHistoryLog.CashOutInfo = new List<CashOutInfo>();
        //        gameHistoryLog.FreeGames = new List<FreeGame>
        //        {
        //            new FreeGame{FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue},
        //            new FreeGame{FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)}

        //        };
        //        gameHistoryLog.Jackpots = new[]
        //        {
        //            new JackpotInfo { TransactionId = 62, LevelId = 15, WinAmount = 10 },
        //            new JackpotInfo { TransactionId = 64, LevelId = 16, WinAmount = 30 },
        //            new JackpotInfo { TransactionId = 66, LevelId = 17, WinAmount = 20 }
        //        };
        //        gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
        //        A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
        //        A.CallTo(() => _currencyHandler.Transactions).Returns(new List<TransactionInfo> { new TransactionInfo { TransactionType = typeof(long), Amount = 218, Time = new DateTime(2002, 12, 14) } });
        //    }

        //    var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);
        //    A.CallTo(() => _currencyHandler.Reset()).Returns(resetAmount);

        //    gameHistory.AssociateTransactions(new List<ITransaction>(){transaction}, applyToFreeGame);

        //    var privateObject = new PrivateObject(gameHistory.CurrentLog);
        //    var transactions = privateObject.GetFieldOrProperty("Transactions");

        //    ((IEnumerable<TransactionInfo>)transactions).Count().Should().Be(2);
        //    ((IEnumerable<TransactionInfo>)transactions).ToArray()[0].Amount.Should().Be(202);
        //    ((IEnumerable<TransactionInfo>)transactions).ToArray()[0].TransactionType.Should().Be(typeof(int));
        //    ((IEnumerable<TransactionInfo>)transactions).ToArray()[1].Amount.Should().Be(expectedTransactionAmount);
        //    ((IEnumerable<TransactionInfo>)transactions).ToArray()[1].TransactionType.Should().Be(transaction.GetType());
        //    gameHistory.CurrentLog.AmountOut.Should().Be(expectedLogAmountOut);
        //    gameHistory.CurrentLog.FreeGames.Count().Should().Be(2);
        //    var lastFreeGame = gameHistory.CurrentLog.FreeGames.LastOrDefault(g => g.EndDateTime != DateTime.MinValue);
        //    lastFreeGame.AmountOut.Should().Be(expectedFreeGameAmountOut);
        //    A.CallTo(() => _currencyHandler.Reset()).MustHaveHappened();
        //}


        [TestMethod]
        public void CompleteCashOut_ShouldCompleteCashOutInfo_WhenFindIt()
        {
            var referenceId = Guid.NewGuid();

            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.CashOutInfo = new List<CashOutInfo>
                {
                    new CashOutInfo
                    {
                        Amount = 500,
                        TraceId = Guid.NewGuid(),
                        Complete = false,
                        Handpay = true,
                        Reason = TransferOutReason.LargeWin,
                        AssociatedTransactions = new[] { 404L }
                    },
                    new CashOutInfo
                    {
                        Amount = 502,
                        TraceId = referenceId,
                        Complete = false,
                        Handpay = true,
                        Reason = TransferOutReason.CashOut,
                        AssociatedTransactions = new[] { 406L }
                    }
                };
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.GameRoundDescriptions = "GameRoundDescriptions";
                gameHistoryLog.FreeGameIndex = 20;
                gameHistoryLog.UncommittedWin = 90;

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            // Nothing has been completed.
            gameHistory.CurrentLog.CashOutInfo.Count(e => e.Complete == false).Should().Be(2);

            gameHistory.CompleteCashOut(referenceId);

            // Only that CashOutInfo with referenceId should be completed.
            gameHistory.CurrentLog.CashOutInfo.Count(e => e.Complete == true).Should().Be(1);
        }

        [TestMethod]
        public void CompleteCashOut_ShouldDoNothing_WhenCantFindIt()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.CashOutInfo = new List<CashOutInfo>
                {
                    new CashOutInfo
                    {
                        Amount = 500,
                        TraceId = Guid.NewGuid(),
                        Complete = false,
                        Handpay = true,
                        Reason = TransferOutReason.LargeWin,
                        AssociatedTransactions = new[] { 404L }
                    },
                    new CashOutInfo
                    {
                        Amount = 502,
                        TraceId = Guid.NewGuid(),
                        Complete = false,
                        Handpay = true,
                        Reason = TransferOutReason.CashOut,
                        AssociatedTransactions = new[] { 406L }
                    }
                };
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.GameRoundDescriptions = "GameRoundDescriptions";
                gameHistoryLog.FreeGameIndex = 20;
                gameHistoryLog.UncommittedWin = 90;

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.CompleteCashOut(Guid.NewGuid());

            gameHistory.CurrentLog.CashOutInfo.Count(e => e.Complete == false).Should().Be(2);
        }


        [TestMethod]
        public void ClearForRecovery_ShouldClearData_WhenInNotReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.GameRoundDescriptions = "GameRoundDescriptions";
                gameHistoryLog.FreeGameIndex = 20;
                gameHistoryLog.UncommittedWin = 90;

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.ClearForRecovery();
            var privateObject = new PrivateObject(gameHistory.CurrentLog);
            privateObject.GetFieldOrProperty("FreeGameIndex").Should().Be(0);
            gameHistory.CurrentLog.UncommittedWin.Should().Be(0);
            gameHistory.CurrentLog.GameRoundDescriptions.Should().Be(string.Empty);
        }

        [TestMethod]
        public void ClearForRecovery_ShouldNotClearData_WhenInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.GameRoundDescriptions = "GameRoundDescriptions";
                gameHistoryLog.FreeGameIndex = 20;
                gameHistoryLog.UncommittedWin = 90;

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.ClearForRecovery();
            var privateObject = new PrivateObject(gameHistory.CurrentLog);
            privateObject.GetFieldOrProperty("FreeGameIndex").Should().Be(20);
            gameHistory.CurrentLog.UncommittedWin.Should().Be(90);
            gameHistory.CurrentLog.GameRoundDescriptions.Should().Be("GameRoundDescriptions");
        }

        [TestMethod]
        public void LogFatalError_ShouldUpdateErrorCodeAndPlayState_WhenNotInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.LogFatalError(ErrorCode.LegitimacyLimit);
            var privateObject = new PrivateObject(gameHistory.CurrentLog);
            privateObject.GetFieldOrProperty("ErrorCode").Should().Be(ErrorCode.LegitimacyLimit);
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.FatalError);
        }

        [TestMethod]
        public void LogFatalError_ShouldNotUpdateErrorCodeAndPlayState_WhenInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.LogFatalError(ErrorCode.LegitimacyLimit);
            var privateObject = new PrivateObject(gameHistory.CurrentLog);
            privateObject.GetFieldOrProperty("ErrorCode").Should().Be(ErrorCode.NoError);
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.PrimaryGameStarted);
        }

        [TestMethod]
        public void EndGame_ShouldEndFreeGameAndUpdateCredits_WhenNotInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 6;
                gameHistoryLog.EndCredits = 10;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)
                    }
                };

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _bank.QueryBalance()).Returns(677);
            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.EndGame();
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.GameEnded);
            gameHistory.CurrentLog.EndCredits.Should().Be(677);
            gameHistory.CurrentLog.LastCommitIndex.Should().Be(6);
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().EndDateTime.Should().NotBe(DateTime.MinValue);
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().EndCredits.Should().Be(677);
        }

        [TestMethod]
        public void EndGame_ShouldNotEndFreeGameAndUpdateCredits_WhenInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 6;
                gameHistoryLog.EndCredits = 10;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)
                    }
                };

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);

                //A.CallTo(() => aFake.AMethod(anInt, ref aRef, out anOut)).Returns(true)
                //    .AssignsOutAndRefParametersLazily((int someInt, string someRef, string someOut) =>
                //        new[] { "new aRef value: " + someInt, "new anOut value" });

                //A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog))
                //    .Returns(true).AssignsOutAndRefParametersLazily((int someInt, string someOut) =>
                //        new[] { new GameHistoryLog() });


            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _bank.QueryBalance()).Returns(677);
            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.EndGame();
            gameHistory.CurrentLog.PlayState.Should().NotBe(PlayState.GameEnded);
            gameHistory.CurrentLog.EndCredits.Should().Be(10);
            gameHistory.CurrentLog.LastCommitIndex.Should().Be(0);
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().EndDateTime.Should().Be(DateTime.MinValue);
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().EndCredits.Should().Be(700);
        }

        [TestMethod]
        public void EndFreeGame_ShouldNotEndFreeGameAndUpdateCredits_WhenInReplay()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 6;
                gameHistoryLog.EndCredits = 10;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime = DateTime.MinValue
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25, 1, 0, 2)
                    }
                };

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _bank.QueryBalance()).Returns(677);
            A.CallTo(() => _replay.IsActive).Returns(true);

            gameHistory.EndFreeGame();
            gameHistory.CurrentLog.PlayState.Should().NotBe(PlayState.GameEnded);
            gameHistory.CurrentLog.EndCredits.Should().Be(10);
            gameHistory.CurrentLog.LastCommitIndex.Should().Be(0);
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().EndDateTime.Should().Be(DateTime.MinValue);
            gameHistory.CurrentLog.FreeGames.FirstOrDefault().EndCredits.Should().Be(700);
        }

        [TestMethod]
        public void EndFreeGame_ShouldNotEndFreeGameAndUpdateCredits_WhenNotInReplayAndNotGameResultWithNone()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 6;
                gameHistoryLog.EndCredits = 10;
                gameHistoryLog.FreeGames = new List<FreeGame>
                {
                    new FreeGame
                    {
                        FinalWin = 10, AmountOut = 200, StartCredits = 670, EndCredits = 700,
                        StartDateTime = new DateTime(2003, 12, 25), EndDateTime =  new DateTime(2002, 12, 25)
                    },
                    new FreeGame
                    {
                        FinalWin = 190, AmountOut = 40, StartCredits = 400, EndCredits = 500,
                        StartDateTime = new DateTime(2002, 12, 25), EndDateTime = new DateTime(2002, 12, 25)
                    }
                };

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _bank.QueryBalance()).Returns(677);
            A.CallTo(() => _replay.IsActive).Returns(false);

            var response = gameHistory.EndFreeGame();
            response.Should().BeNull();
            gameHistory.CurrentLog.PlayState.Should().NotBe(PlayState.GameEnded);
            gameHistory.CurrentLog.EndCredits.Should().Be(10);
            gameHistory.CurrentLog.LastCommitIndex.Should().Be(0);
        }

        [TestMethod]
        public void EndFreeGame_ShouldNotEndFreeGameAndUpdateCredits_WhenNoFreeGamesExist()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.FreeGameIndex = 6;
                gameHistoryLog.EndCredits = 10;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _bank.QueryBalance()).Returns(677);
            A.CallTo(() => _replay.IsActive).Returns(false);

            var response = gameHistory.EndFreeGame();
            response.Should().BeNull();
            gameHistory.CurrentLog.PlayState.Should().NotBe(PlayState.GameEnded);
            gameHistory.CurrentLog.EndCredits.Should().Be(10);
            gameHistory.CurrentLog.LastCommitIndex.Should().Be(0);
        }

        [TestMethod]
        public void EndSecondaryGame_ShouldUpdatePlayStateAndSecondaryPlayed_WhenIsNotReplaying()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.InitialWin = 120;
                gameHistoryLog.SecondaryPlayed = 88;
                gameHistoryLog.SecondaryWin = 400;
                gameHistoryLog.SecondaryWager = 500;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            A.CallTo(() => _replay.IsActive).Returns(false);

            gameHistory.EndSecondaryGame(200);

            gameHistory.CurrentLog.SecondaryPlayed.Should().Be(89);
            gameHistory.CurrentLog.SecondaryWin.Should().Be(600);
            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.SecondaryGameEnded);
        }

        [TestMethod]
        public void End_ShouldIncreaseIndexAndSetPlayState_WhenPlayStateNotIdle()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();

                if (i == 0)
                    gameHistoryLog.TransactionId = MaxLogsCount + 1;
                else
                    gameHistoryLog.TransactionId = i;

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);
            var privateObject = new PrivateObject(gameHistory);
            var currentLogIndex = privateObject.GetFieldOrProperty("CurrentLogIndex");

            currentLogIndex.Should().Be(0);

            gameHistory.End();

            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.Idle);

            currentLogIndex = privateObject.GetFieldOrProperty("CurrentLogIndex");
            currentLogIndex.Should().Be(1);
        }

        [TestMethod]
        public void End_ShouldRollIndexAndSetPlayState_WhenReachMaxCount()
        {
            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();
                gameHistoryLog.TransactionId = i;
                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            gameHistory.End();

            gameHistory.CurrentLog.PlayState.Should().Be(PlayState.Idle);

            var privateObject = new PrivateObject(gameHistory);
            privateObject.GetFieldOrProperty("CurrentLogIndex").Should().Be(0);
        }

        [TestMethod]
        public void Start_ShouldCreateNewLogSuccessfully_WhenPlayStateNotIdle()
        {
            long queryBalance = 1500;
            long initialWager = 2000;
            long amountIn = 219;
            int gameId = 2;
            long denom = 100;
            long nextTransactionId = 6000;
            long nextLogSequence = 12000;
            string localeCode = "code";

            A.CallTo(() => _properties.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(denom);
            A.CallTo(() => _properties.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(gameId);
            A.CallTo(() => _properties.GetProperty(GamingConstants.SelectedLocaleCode, "en-us")).Returns(localeCode);

            A.CallTo(() => _currencyHandler.Reset()).Returns(amountIn);
            A.CallTo(() => _bank.QueryBalance()).Returns(queryBalance);

            A.CallTo(() => _idProvider.GetNextTransactionId()).Returns(nextTransactionId);
            A.CallTo(() => _idProvider.GetNextLogSequence<IGameHistory>()).Returns(nextLogSequence);

            for (int i = 0; i < MaxLogsCount; i++)
            {
                var gameHistoryLog = A.Fake<GameHistoryLog>();

                if (i == 0)
                    gameHistoryLog.TransactionId = MaxLogsCount + 1;
                else
                    gameHistoryLog.TransactionId = i;

                gameHistoryLog.PlayState = PlayState.PrimaryGameStarted;
                A.CallTo(() => _persistentBlock.GetValue(i, out gameHistoryLog)).Returns(true);
            }

            var gameHistory = new GameHistory(_properties, _bank, _replay, _idProvider, _systemDisableManager, _currencyHandler, _persistenceProvider);

            gameHistory.Start(initialWager, new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null);

            var currentLog = gameHistory.CurrentLog;

            currentLog.TransactionId.Should().Be(nextTransactionId);
            currentLog.LogSequence.Should().Be(nextLogSequence);
            currentLog.PlayState.Should().Be(PlayState.PrimaryGameStarted);
            currentLog.GameId.Should().Be(gameId);

            currentLog.FinalWager.Should().Be(initialWager);
            currentLog.InitialWager.Should().Be(initialWager);

            currentLog.StartCredits.Should().Be(queryBalance);
            currentLog.LastCommitIndex.Should().Be(-1);
            var privateObject = new PrivateObject(currentLog);
            privateObject.GetFieldOrProperty("FreeGameIndex").Should().Be(0);
            currentLog.LocaleCode.Should().Be(localeCode);
            currentLog.EndDateTime.Should().Be(DateTime.MinValue);
            currentLog.EndCredits.Should().Be(0L);
            currentLog.InitialWin.Should().Be(0L);
            currentLog.SecondaryPlayed.Should().Be(0L);
            currentLog.SecondaryWager.Should().Be(0L);
            currentLog.SecondaryWin.Should().Be(0L);
            currentLog.FinalWin.Should().Be(0L);
            currentLog.UncommittedWin.Should().Be(0);
            currentLog.AmountOut.Should().Be(0);
            currentLog.GameRoundDescriptions.Should().Be(string.Empty);
            currentLog.Jackpots.Count().Should().Be(0);
            currentLog.JackpotSnapshot.Count().Should().Be(0);
            currentLog.FreeGames.Count().Should().Be(0);
        }
    }
    */
}