namespace Aristocrat.Monaco.Bingo.Tests.Services.GamePlay
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Accounting.Contracts;
    using Application.Contracts;
    using Application.Contracts.Extensions;
    using Aristocrat.Bingo.Client.Messages;
    using Aristocrat.Bingo.Client.Messages.GamePlay;
    using Aristocrat.Monaco.Test.Common;
    using Bingo.GameEndWin;
    using Bingo.Services.GamePlay;
    using Commands;
    using Common;
    using Common.Events;
    using Common.Exceptions;
    using Common.GameOverlay;
    using Common.Storage;
    using Common.Storage.Model;
    using Gaming.Contracts;
    using Gaming.Contracts.Central;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Protocol.Common.Storage.Entity;
    using GameEndWinFactory = Common.IBingoStrategyFactory<Bingo.GameEndWin.IGameEndWinStrategy, Common.Storage.Model.GameEndWinStrategy>;

    [TestClass]
    public class CentralHandlerTests
    {
        private readonly Mock<IEventBus> _eventBus = new(MockBehavior.Default);
        private readonly Mock<IPlayerBank> _playerBank = new(MockBehavior.Default);
        private readonly Mock<ICentralProvider> _centralProvider = new(MockBehavior.Default);
        private readonly Mock<IGameProvider> _gameProvider = new(MockBehavior.Default);
        private readonly Mock<IGamePlayState> _gamePlayState = new(MockBehavior.Default);
        private readonly Mock<IPropertiesManager> _properties = new(MockBehavior.Default);
        private readonly Mock<ISystemDisableManager> _disable = new(MockBehavior.Default);
        private readonly Mock<IBingoCardProvider> _bingCardProvider = new(MockBehavior.Default);
        private readonly Mock<GameEndWinFactory> _gewFactory = new(MockBehavior.Default);
        private readonly Mock<ICommandHandlerFactory> _commandHandlerFactory = new(MockBehavior.Default);
        private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new(MockBehavior.Default);

        private const uint CardSerial = 1235;

        private CentralHandler _target;
        private BingoCard _bingoCard;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var dateTime = new DateTime(2000);
            var outcome =
                new CentralTransaction(2, dateTime, 3, 1, "standard", 1000, 4)
                {
                    OutcomeState = OutcomeState.Committed
                };

            _centralProvider.Setup(x => x.Transactions).Returns(new List<CentralTransaction> { outcome });

            _target = CreateTarget();
            SetupBingoCard();
        }

        [DataRow(true, false, false, false, false, false, false, false, false, false, false, DisplayName="Null IEventBus")]
        [DataRow(false, true, false, false, false, false, false, false, false, false, false, DisplayName="Null IPlayerBank")]
        [DataRow(false, false, true, false, false, false, false, false, false, false, false, DisplayName="Null ICentralProvider")]
        [DataRow(false, false, false, true, false, false, false, false, false, false, false, DisplayName="Null IGameProvider")]
        [DataRow(false, false, false, false, true, false, false, false, false, false, false, DisplayName="Null IGamePlayState")]
        [DataRow(false, false, false, false, false, true, false, false, false, false, false, DisplayName="Null IPropertiesManager")]
        [DataRow(false, false, false, false, false, false, true, false, false, false, false, DisplayName="Null ISystemDisableManager")]
        [DataRow(false, false, false, false, false, false, false, true, false, false, false, DisplayName="Null IBingoCardProvider")]
        [DataRow(false, false, false, false, false, false, false, false, true, false, false, DisplayName="Null GameEndWinFactory")]
        [DataRow(false, false, false, false, false, false, false, false, false, true, false, DisplayName="Null ICommandHandlerFactory")]
        [DataRow(false, false, false, false, false, false, false, false, false, false, true, DisplayName="Null IUnitOfWorkFactory")]
        [DataTestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullConstructorArgumentTest(
            bool nullEventBus,
            bool nullBank,
            bool nullCentralProvider,
            bool nullGames,
            bool nullGamePlay,
            bool nullProperties,
            bool nullDisable,
            bool nullBingoCard,
            bool nullGewFactory,
            bool nullCommandFactory,
            bool nullUnitOfWorkFactory)
        {
            _target = CreateTarget(
                nullEventBus,
                nullBank,
                nullCentralProvider,
                nullGames,
                nullGamePlay,
                nullProperties,
                nullDisable,
                nullBingoCard,
                nullGewFactory,
                nullCommandFactory,
                nullUnitOfWorkFactory);
        }

        [TestMethod]
        public async Task SendCombineOutcomesEventTest()
        {
            const long gameSerial = 123;
            const long largeWinLimit = 10L;
            var ballCall = new List<int> { 12, 32, 45, 51, 23, 72, 23, 72 };
            const long winAmount = 100;
            const JackpotDetermination strategy = JackpotDetermination.InterimPattern;
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            var model = new BingoServerSettingsModel { JackpotAmountDetermination = strategy };
            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(model);
            _commandHandlerFactory.Setup(x => x.Execute(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _eventBus.Setup(x => x.Publish(It.IsAny<AllowCombinedOutcomesEvent>())).Verifiable();
            _eventBus.Setup(x => x.Publish(It.IsAny<BingoGamePatternEvent>())).Verifiable();
            _properties.Setup(m => m.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit)).Returns(largeWinLimit);
            using var source = new CancellationTokenSource();

            _bingCardProvider.Setup(x => x.GetCardBySerial((int)CardSerial)).Returns(_bingoCard);

            var bingoDetails = new GameOutcomeBingoDetails(
                0,
                new CardPlayed[] { new(CardSerial, int.MaxValue, true) },
                ballCall,
                0);
            var gameDetails = new GameOutcomeGameDetails(0, 123, 123, 1, "Test Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                winAmount,
                string.Empty,
                new WinResult[]
                {
                    new(12345, winAmount, 30, int.MaxValue, 1234, "4 Corners", (int)CardSerial, false, 3)
                });
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false);
            await _target.ProcessGameOutcome(outcome, source.Token);

            _eventBus.Verify();
        }

        [TestMethod]
        public async Task RequestOutcomesTest()
        {
            var currentTransactionGameId = 3;
            var defaultBetDetails = new BetDetails(0, 0, 0, 0, 0);
            var machineSerial = "123";
            Mock<IGameDetail> gameDetail = new(MockBehavior.Default);

            var dateTime = new DateTime(2000);

            var outcome = new CentralTransaction(2, dateTime, currentTransactionGameId, 1, "standard", 1000, 4);
            _gamePlayState.Setup(x => x.SetGameEndHold(true)).Verifiable();
            _gameProvider.Setup(x => x.GetGame(currentTransactionGameId)).Returns(gameDetail.Object).Verifiable();
            _properties.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns(machineSerial).Verifiable();
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedBetDetails, It.IsAny<BetDetails>())).Returns(defaultBetDetails).Verifiable();
            _commandHandlerFactory.Setup(x => x.Execute(It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            gameDetail.Setup(x => x.ThemeName).Returns("Theme1");

            await _target.RequestOutcomes(outcome);

            _properties.Verify();
            _eventBus.Verify();
            _gameProvider.Verify();
            _gamePlayState.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RequestOutcomesNullTest()
        {
            await _target.RequestOutcomes(null);
        }

        [TestMethod]
        public async Task ProcessGameOutcomeFailTest()
        {
            const long gameSerial = 123;
            var ballCall = new List<int> { 12, 32, 45, 51, 23, 72, 23, 72 };
            const long winAmount = 100;
            MoqServiceManager.CreateInstance(MockBehavior.Default);

            _gamePlayState.Setup(x => x.InGameRound).Returns(true);

            using var source = new CancellationTokenSource();
            var bingoDetails = new GameOutcomeBingoDetails(
                0,
                new CardPlayed[] { new(CardSerial, int.MaxValue, true) },
                ballCall,
                0);
            var gameDetails = new GameOutcomeGameDetails(0, 123, 123, 1, "Test Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                winAmount,
                string.Empty,
                new WinResult[]
                {
                    new(12345, winAmount, 30, int.MaxValue, 1234, "4 Corners", (int)CardSerial, false, 3)
                });
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false);

            var playTask = _target.ProcessGameOutcome(outcome, source.Token);

            await playTask;

            Assert.AreEqual(false, playTask.Result);

            _eventBus.Verify();
        }

        [DataRow(true, false, true, true, DisplayName = "Game Round is not active")]
        [DataRow(true, true, false, true, DisplayName = "No GEW Pattern was found")]
        [ExpectedException(typeof(BingoGamePlayException))]
        [DataTestMethod]
        public async Task ClaimGEWBingoGamePlayExceptionTest(
            bool gewAccepted,
            bool inGameRound,
            bool hasGewPattern,
            bool successfulClaim)
        {
            const long gameSerial = 123;
            const long gewWinAmount = 100;
            var ballCall = new List<int>
            {
                12,
                32,
                45,
                51,
                23,
                72,
                23,
                72
            };
            const GameEndWinStrategy strategy = GameEndWinStrategy.BonusCredits;

            var model = new BingoServerSettingsModel { GameEndingPrize = strategy };
            _gamePlayState.Setup(x => x.InGameRound).Returns(inGameRound);
            var gewStrategy = new Mock<IGameEndWinStrategy>(MockBehavior.Default);
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(model);
            gewStrategy.Setup(x => x.ProcessWin(gewWinAmount.CentsToMillicents(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(successfulClaim));
            _gewFactory.Setup(x => x.Create(strategy)).Returns(gewStrategy.Object);
            _commandHandlerFactory.Setup(x => x.Execute(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            using var source = new CancellationTokenSource();
            var playTask = hasGewPattern
                ? SetupGEWGamePlay(gameSerial, gewWinAmount, ballCall, source.Token)
                : Task.CompletedTask;

            await _target.ProcessClaimWin(
                new ClaimWinResults(ResponseCode.Ok, gewAccepted, gameSerial, CardSerial),
                source.Token);
            await playTask;
        }

        [DataRow(false, true, true, true, false, DisplayName = "GEW was denied test")]
        [DataRow(true, true, true, true, true, DisplayName = "GEW was accepted and awarded")]
        [DataRow(true, true, true, false, false, DisplayName = "GEW was accepted but failed to be awarded")]
        [DataTestMethod]
        public async Task ClaimGEWTest(
            bool gewAccepted,
            bool inGameRound,
            bool hasGewPattern,
            bool successfulClaim,
            bool expectedResult)
        {
            const long gameSerial = 123;
            const long gewWinAmount = 100;
            var ballCall = new List<int> { 12, 32, 45, 51, 23, 72, 23, 72 };
            const GameEndWinStrategy strategy = GameEndWinStrategy.BonusCredits;

            var model = new BingoServerSettingsModel { GameEndingPrize = strategy };
            _gamePlayState.Setup(x => x.InGameRound).Returns(inGameRound);
            var gewStrategy = new Mock<IGameEndWinStrategy>(MockBehavior.Default);
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(model);
            gewStrategy.Setup(x => x.ProcessWin(gewWinAmount.CentsToMillicents(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(successfulClaim));
            _gewFactory.Setup(x => x.Create(strategy)).Returns(gewStrategy.Object);
            _commandHandlerFactory.Setup(x => x.Execute(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            using var source = new CancellationTokenSource();
            var playTask = hasGewPattern
                ? SetupGEWGamePlay(gameSerial, gewWinAmount, ballCall, source.Token)
                : Task.CompletedTask;

            var result = await _target.ProcessClaimWin(
                new ClaimWinResults(ResponseCode.Ok, gewAccepted, gameSerial, CardSerial),
                source.Token);

            Assert.AreEqual(expectedResult, result);
            await playTask;
        }

        private CentralHandler CreateTarget(
            bool nullEventBus = false,
            bool nullBank = false,
            bool nullCentralProvider = false,
            bool nullGames = false,
            bool nullGamePlay = false,
            bool nullProperties = false,
            bool nullDisable = false,
            bool nullBingoCard = false,
            bool nullGewFactory = false,
            bool nullCommandFactory = false,
            bool nullUnitOfWorkFactory = false)
        {
            return new(
                nullEventBus ? null : _eventBus.Object,
                nullBank ? null : _playerBank.Object,
                nullCentralProvider ? null : _centralProvider.Object,
                nullGames ? null : _gameProvider.Object,
                nullGamePlay ? null : _gamePlayState.Object,
                nullProperties ? null : _properties.Object,
                nullDisable ? null : _disable.Object,
                nullBingoCard ? null : _bingCardProvider.Object,
                nullGewFactory ? null : _gewFactory.Object,
                nullCommandFactory ? null : _commandHandlerFactory.Object,
                nullUnitOfWorkFactory ? null : _unitOfWorkFactory.Object);
        }

        private void SetupBingoCard()
        {
            if (_bingoCard is not null)
            {
                return;
            }

            _bingoCard = new BingoCard(CardSerial);

            for (var i = 0; i < BingoConstants.BingoCardDimension; i++)
            {
                for (var j = 0; j < BingoConstants.BingoCardDimension; j++)
                {
                    _bingoCard.Numbers[i, j] = new BingoNumber(0, BingoNumberState.CardInitial);
                }
            }
        }

        private async Task SetupGEWGamePlay(
            long gameSerial,
            long gewWinAmount,
            IEnumerable<int> ballCall,
            CancellationToken token)
        {
            _bingCardProvider.Setup(x => x.GetCardBySerial((int)CardSerial)).Returns(_bingoCard);
            _centralProvider.Setup(x => x.Transactions)
                .Returns(
                    new List<CentralTransaction>
                    {
                        new(0, DateTime.Now, 123, 1000, string.Empty, 100, 1)
                        {
                            Descriptions = new List<IOutcomeDescription>
                            {
                                new BingoGameDescription
                                {
                                    Patterns = new List<BingoPattern>
                                    {
                                        new(
                                            "GEW Pattern",
                                            1,
                                            CardSerial,
                                            gewWinAmount,
                                            40,
                                            1234,
                                            true,
                                            0x80,
                                            1)
                                    }
                                }
                            }
                        }
                    });

            var bingoDetails = new GameOutcomeBingoDetails(
                0,
                new CardPlayed[] { new(CardSerial, int.MaxValue, true) },
                ballCall.ToList(),
                0);
            var gameDetails = new GameOutcomeGameDetails(0, 123, 123, 1, "Test Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                gewWinAmount,
                string.Empty,
                new WinResult[]
                {
                    new(12345, gewWinAmount, 50, int.MaxValue, 1234, "GEW Pattern", (int)CardSerial, true, 1)
                });
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false);

            await _target.ProcessGameOutcome(outcome, token);
        }
    }
}