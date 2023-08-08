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
        private const uint SideBetCardSerial = 2345;

        private CentralHandler _target;
        private BingoCard _bingoCard;
        private BingoCard _sideBetBingoCard;

        [TestInitialize]
        public void MyTestInitialize()
        {
            var dateTime = new DateTime(2000);

            var mainGameInfo = new AdditionalGamePlayInfo(0, 3, 1, 1000, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var outcome =
                new CentralTransaction(2, dateTime, 3, "standard", 4, gamePlayInfo)
                {
                    OutcomeState = OutcomeState.Committed,
                };

            _centralProvider.Setup(x => x.Transactions).Returns(new List<CentralTransaction> { outcome });

            _target = CreateTarget();
            SetupBingoCard();
            SetupSideBetBingoCard();
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
            var gameDetails = new GameOutcomeGameDetails("0", 123, 123, 1, "Test Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                winAmount,
                string.Empty,
                new WinResult[]
                {
                    new(12345, winAmount, 30, int.MaxValue, 1234, "4 Corners", (int)CardSerial, false, 3, Enumerable.Empty<string>())
                });
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false, 0);
            var outcomes = new GameOutcomes(ResponseCode.Ok, new List<GameOutcome> { outcome });
            await _target.ProcessGameOutcomes(outcomes, source.Token);

            _eventBus.Verify();
        }

        [TestMethod]
        public async Task RequestOutcomesTest()
        {
            var currentTransactionGameId = 3;
            var defaultBetDetails = new BetDetails(0, 0, 0, 0, 0, 0, 0, 0);
            var machineSerial = "123";
            Mock<IGameDetail> gameDetail = new(MockBehavior.Default);
            var dateTime = new DateTime(2000);

            var mainGameInfo = new AdditionalGamePlayInfo(0, currentTransactionGameId, 1, 1000, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var outcome = new CentralTransaction(2, dateTime, currentTransactionGameId, "standard", 4, gamePlayInfo);
            _gamePlayState.Setup(x => x.SetGameEndHold(true)).Verifiable();
            _gameProvider.Setup(x => x.GetGame(currentTransactionGameId)).Returns(gameDetail.Object).Verifiable();

            _properties.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns(machineSerial).Verifiable();
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedMultiGameBetDetails, It.IsAny<IEnumerable<BetDetails>>())).Returns(new[] { defaultBetDetails }).Verifiable();
            _commandHandlerFactory.Setup(x => x.Execute(It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            gameDetail.Setup(x => x.ThemeName).Returns("Theme1");

            await _target.RequestOutcomes(outcome);

            _properties.Verify();
            _eventBus.Verify();
            _gameProvider.Verify();
            _gamePlayState.Verify();
        }

        [ExpectedException(typeof(ArgumentNullException))]
        public async Task RequestOutcomesNullTest()
        {
            await _target.RequestOutcomes(null);
        }

        [TestMethod]
        public async Task ProcessGameOutcomesFailTest()
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
            var gameDetails = new GameOutcomeGameDetails("0", 123, 123, 1, "Test Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                winAmount,
                string.Empty,
                new WinResult[]
                {
                    new(12345, winAmount, 30, int.MaxValue, 1234, "4 Corners", (int)CardSerial, false, 3, Enumerable.Empty<string>())
                });
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false, 0);
            var outcomes = new GameOutcomes(ResponseCode.Ok, new List<GameOutcome> { outcome });

            var playTask = _target.ProcessGameOutcomes(outcomes, source.Token);

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

        [TestMethod]
        public async Task SingleGameNoBallCallTest()
        {
            const long gameSerial = 123;
            const long winAmount = 100;
            var ballCall = new List<int>();
            using var source = new CancellationTokenSource();
            const long largeWinLimit = 10L;

            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
            const JackpotDetermination strategy = JackpotDetermination.InterimPattern;
            var model = new BingoServerSettingsModel { JackpotAmountDetermination = strategy };
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(model);
            _properties.Setup(m => m.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit)).Returns(largeWinLimit);

            // Must first request outcomes
            RequestOutcomes();

            await SetupSingleGamePlayNoBallCall(gameSerial, winAmount, ballCall, source.Token);

            _eventBus.Verify(x => x.Publish(It.IsAny<AllowCombinedOutcomesEvent>()), Times.Never);
            _eventBus.Verify(x => x.Publish(It.IsAny<PlayersFoundEvent>()), Times.Never());
            _eventBus.Verify(x => x.Publish(It.IsAny<BingoGamePatternEvent>()), Times.Once());
        }

        [TestMethod]
        public async Task SingleGameTest()
        {
            const long gameSerial = 123;
            const long winAmount = 100;
            var ballCall = new List<int> { 12, 32, 45, 51, 23, 72, 23, 72 };
            using var source = new CancellationTokenSource();
            const long largeWinLimit = 10L;

            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
            const JackpotDetermination strategy = JackpotDetermination.InterimPattern;
            var model = new BingoServerSettingsModel { JackpotAmountDetermination = strategy };
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(model);
            _properties.Setup(m => m.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit)).Returns(largeWinLimit);

            // Must first request outcomes
            RequestOutcomes();

            // Now you can process game outcomes
            await SetupSingleGamePlay(gameSerial, winAmount, ballCall, source.Token);

            // TODO why is transactionId 0?
            //_centralProvider.Verify(x => x.OutcomeResponse(0, It.IsAny<IReadOnlyCollection<Outcome>>(), OutcomeException.None, It.IsAny<IEnumerable<BingoGameDescription>>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<AllowCombinedOutcomesEvent>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<PlayersFoundEvent>()), Times.Once());
            _eventBus.Verify(x => x.Publish(It.IsAny<BingoGamePatternEvent>()), Times.Once());
        }

        [TestMethod]
        public async Task SingleGameUpdateTest()
        {
            const long gameSerial = 123;
            const long winAmount = 100;
            var ballCall = new List<int> { 12, 32, 45, 51, 23, 72, 23, 72 };
            var ballCallUpdate = new List<int> { 12, 32, 45, 51, 23, 72, 23, 72, 5 };
            using var source = new CancellationTokenSource();
            const long largeWinLimit = 10L;

            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
            const JackpotDetermination strategy = JackpotDetermination.InterimPattern;
            var model = new BingoServerSettingsModel { JackpotAmountDetermination = strategy };
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(model);
            _properties.Setup(m => m.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit)).Returns(largeWinLimit);

            // Must first request outcomes
            RequestOutcomes();

            // Now you can process game outcomes
            await SetupSingleGamePlay(gameSerial, winAmount, ballCall, source.Token);

            // TODO why is transactionId 0?
            //_centralProvider.Verify(x => x.OutcomeResponse(0, It.IsAny<IReadOnlyCollection<Outcome>>(), OutcomeException.None, It.IsAny<IEnumerable<BingoGameDescription>>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<AllowCombinedOutcomesEvent>()), Times.Once);
            //_eventBus.Verify(x => x.Publish(It.IsAny<PlayersFoundEvent>()), Times.Once());
            _eventBus.Verify(x => x.Publish(It.IsAny<BingoGamePatternEvent>()), Times.Once());

            // Call it again to perform an update call
            await SetupSingleGamePlay(gameSerial, winAmount, ballCallUpdate, source.Token);

            _centralProvider.Verify(x => x.UpdateOutcomeDescription(0, It.IsAny<IEnumerable<BingoGameDescription>>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<AllowCombinedOutcomesEvent>()), Times.Exactly(2));
            _eventBus.Verify(x => x.Publish(It.IsAny<PlayersFoundEvent>()), Times.Once());
            _eventBus.Verify(x => x.Publish(It.IsAny<BingoGamePatternEvent>()), Times.Exactly(2));
        }


        // TODO this test works when run by itself. The ClaimGEWTest is interfering with this test somehow making it fail when the whole test suite is run.
        //[TestMethod]
        public async Task MultiGameTest()
        {
            const long gameSerial = 123;
            const long winAmount = 100;
            const long winAmount2 = 200;
            var ballCall = new List<int> { 12, 32, 45, 51, 23, 72, 23, 72 };
            using var source = new CancellationTokenSource();
            const long largeWinLimit = 10L;

            var mainGameInfo = new AdditionalGamePlayInfo(0, 3, 1, 1000, 0);
            var sideBetGameInfo = new AdditionalGamePlayInfo(1, 1, 25, 25, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo, sideBetGameInfo };

            _centralProvider.Reset();
            var dateTime = new DateTime(2000);
            var outcome =
                new CentralTransaction(2, dateTime, 3, "standard", 2, gamePlayInfo)
                {
                    OutcomeState = OutcomeState.Committed,
                };

            _centralProvider.Setup(x => x.Transactions).Returns(new List<CentralTransaction> { outcome });

            _gamePlayState.Setup(x => x.InGameRound).Returns(true);
            const JackpotDetermination strategy = JackpotDetermination.InterimPattern;
            var model = new BingoServerSettingsModel { JackpotAmountDetermination = strategy };
            _unitOfWorkFactory.Setup(x => x.Invoke(It.IsAny<Func<IUnitOfWork, BingoServerSettingsModel>>()))
                .Returns(model);
            _properties.Setup(m => m.GetProperty(AccountingConstants.LargeWinLimit, AccountingConstants.DefaultLargeWinLimit)).Returns(largeWinLimit);

            // Must first request outcomes
            RequestOutcomes();

            // Now you can process game outcomes
            await SetupMultiGamePlay(gameSerial, winAmount, winAmount2, ballCall, source.Token);

            // TODO why is transactionId 0?
            _centralProvider.Verify(x => x.OutcomeResponse(0, It.IsAny<IReadOnlyCollection<Outcome>>(), OutcomeException.None, It.IsAny<IEnumerable<BingoGameDescription>>()), Times.Once);
            _eventBus.Verify(x => x.Publish(It.IsAny<AllowCombinedOutcomesEvent>()), Times.Once());
            _eventBus.Verify(x => x.Publish(It.IsAny<PlayersFoundEvent>()), Times.Once());
            _eventBus.Verify(x => x.Publish(It.IsAny<BingoGamePatternEvent>()), Times.Exactly(2));
        }

        private async void RequestOutcomes()
        {
            var currentTransactionGameId = 3;
            var defaultBetDetails = new BetDetails(0, 0, 0, 0, 0, 0, 0, 0);
            var machineSerial = "123";
            Mock<IGameDetail> gameDetail = new(MockBehavior.Default);
            var dateTime = new DateTime(2000);

            var mainGameInfo = new AdditionalGamePlayInfo(0, currentTransactionGameId, 1, 1000, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            var outcome = new CentralTransaction(2, dateTime, currentTransactionGameId, "standard", 4, gamePlayInfo);
            _gamePlayState.Setup(x => x.SetGameEndHold(true)).Verifiable();
            _gameProvider.Setup(x => x.GetGame(currentTransactionGameId)).Returns(gameDetail.Object).Verifiable();

            _properties.Setup(x => x.GetProperty(ApplicationConstants.SerialNumber, string.Empty)).Returns(machineSerial).Verifiable();
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedBetDetails, It.IsAny<BetDetails>())).Returns(defaultBetDetails).Verifiable();
            _commandHandlerFactory.Setup(x => x.Execute(It.IsAny<object>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();
            gameDetail.Setup(x => x.ThemeName).Returns("Theme1");

            await _target.RequestOutcomes(outcome);
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

        private void SetupSideBetBingoCard()
        {
            if (_sideBetBingoCard is not null)
            {
                return;
            }

            _sideBetBingoCard = new BingoCard(SideBetCardSerial);

            for (var i = 0; i < BingoConstants.BingoCardDimension; i++)
            {
                for (var j = 0; j < BingoConstants.BingoCardDimension; j++)
                {
                    _sideBetBingoCard.Numbers[i, j] = new BingoNumber(0, BingoNumberState.CardInitial);
                }
            }
        }

        private async Task SetupGEWGamePlay(
            long gameSerial,
            long gewWinAmount,
            IEnumerable<int> ballCall,
            CancellationToken token)
        {
            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            _bingCardProvider.Setup(x => x.GetCardBySerial((int)CardSerial)).Returns(_bingoCard);
            _centralProvider.Setup(x => x.Transactions)
                .Returns(
                    new List<CentralTransaction>
                    {
                        new(0, DateTime.Now, 123, string.Empty, 1, gamePlayInfo)
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
                                    },
                                    GameEndWinEligibility = 1
                                }
                            }
                        }
                    });

            var bingoDetails = new GameOutcomeBingoDetails(
                1,
                new CardPlayed[] { new(CardSerial, int.MaxValue, true) },
                ballCall.ToList(),
                0);
            var gameDetails = new GameOutcomeGameDetails("0", 123, 123, 1, "Test Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                gewWinAmount,
                string.Empty,
                new WinResult[]
                {
                    new(12345, gewWinAmount, 50, int.MaxValue, 1234, "GEW Pattern", (int)CardSerial, true, 1, Enumerable.Empty<string>())
                });
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false, 0);
            var outcomes = new GameOutcomes(ResponseCode.Ok, new List<GameOutcome> { outcome });

            await _target.ProcessGameOutcomes(outcomes, token);
        }

        private async Task SetupSingleGamePlayNoBallCall(
            long gameSerial,
            long winAmount,
            IEnumerable<int> ballCall,
            CancellationToken token)
        {
            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            _bingCardProvider.Setup(x => x.GetCardBySerial((int)CardSerial)).Returns(_bingoCard);
            _centralProvider.Setup(x => x.Transactions)
                .Returns(
                    new List<CentralTransaction>
                    {
                        new(0, DateTime.Now, 123, string.Empty, 1, gamePlayInfo)
                        {
                            Descriptions = new List<IOutcomeDescription>
                            {
                                new BingoGameDescription
                                {
                                    Patterns = new List<BingoPattern>(),
                                    GameEndWinEligibility = 1,
                                    GameIndex = 0
                                }
                            }
                        }
                    }); ;

            var bingoDetails = new GameOutcomeBingoDetails(
                1,
                new CardPlayed[] { new(CardSerial, int.MaxValue, false) },
                ballCall.ToList(),
                0);
            var gameDetails = new GameOutcomeGameDetails("0", 123, 123, 1, "Test Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                winAmount,
                string.Empty,
                new List<WinResult>());
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false, 0);
            var outcomes = new GameOutcomes(ResponseCode.Ok, new List<GameOutcome> { outcome });

            await _target.ProcessGameOutcomes(outcomes, token);
        }

        private async Task SetupSingleGamePlay(
            long gameSerial,
            long winAmount,
            IEnumerable<int> ballCall,
            CancellationToken token)
        {
            var patternName = "Pattern 1";
            var patternId = 12345;
            var ballQuantity = 30;
            var paytableId = 1234;
            var bitFlags = 0x80;
            var winIndex = 1;

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            _bingCardProvider.Setup(x => x.GetCardBySerial((int)CardSerial)).Returns(_bingoCard);
            _centralProvider.Setup(x => x.Transactions)
                .Returns(
                    new List<CentralTransaction>
                    {
                        new(0, DateTime.Now, 123, string.Empty, 1, gamePlayInfo)
                        {
                            Descriptions = new List<IOutcomeDescription>
                            {
                                new BingoGameDescription
                                {
                                    Patterns = new List<BingoPattern>
                                    {
                                        new(
                                            patternName,
                                            patternId,
                                            CardSerial,
                                            winAmount,
                                            ballQuantity,
                                            paytableId,
                                            false,
                                            bitFlags,
                                            winIndex)
                                    },
                                    GameEndWinEligibility = 1,
                                    GameIndex = 0
                                }
                            }
                        }
                    });

            var bingoDetails = new GameOutcomeBingoDetails(
                1,
                new CardPlayed[] { new(CardSerial, int.MaxValue, false) },
                ballCall.ToList(),
                0);
            var gameDetails = new GameOutcomeGameDetails("0", 123, 123, 1, "Test Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                winAmount,
                string.Empty,
                new WinResult[]
                {
                    new(patternId, winAmount, ballQuantity, int.MaxValue, paytableId, patternName, (int)CardSerial, false, winIndex, Enumerable.Empty<string>())
                });
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false, 0);
            var outcomes = new GameOutcomes(ResponseCode.Ok, new List<GameOutcome> { outcome });

            await _target.ProcessGameOutcomes(outcomes, token);
        }

        private async Task SetupMultiGamePlay(
            long gameSerial,
            long winAmount,
            long winAmountSideBet,
            IEnumerable<int> ballCall,
            CancellationToken token)
        {
            var patternName = "Pattern 1";
            var patternId = 12345;
            var ballQuantity = 30;
            var paytableId = 1234;
            var bitFlags = 0x80;
            var winIndex = 1;

            var patternName2 = "Pattern 2";
            var patternId2 = 23456;
            var ballQuantity2 = 35;
            var paytableId2 = 2345;
            var bitFlags2 = 0x80;
            var winIndex2 = 2;

            var mainGameInfo = new AdditionalGamePlayInfo(0, 123, 1000, 100, 0);
            var gamePlayInfo = new List<AdditionalGamePlayInfo> { mainGameInfo };

            _bingCardProvider.Setup(x => x.GetCardBySerial((int)CardSerial)).Returns(_bingoCard);
            _bingCardProvider.Setup(x => x.GetCardBySerial((int)SideBetCardSerial)).Returns(_sideBetBingoCard);
            _centralProvider.Setup(x => x.Transactions)
                .Returns(
                    new List<CentralTransaction>
                    {
                        new(0, DateTime.Now, 123, string.Empty, 1, gamePlayInfo)
                        {
                            Descriptions = new List<IOutcomeDescription>
                            {
                                new BingoGameDescription
                                {
                                    Patterns = new List<BingoPattern>
                                    {
                                        new(
                                            patternName,
                                            patternId,
                                            CardSerial,
                                            winAmount,
                                            ballQuantity,
                                            paytableId,
                                            false,
                                            bitFlags,
                                            winIndex)
                                    },
                                    GameEndWinEligibility = 1,
                                    GameIndex = 0
                                },
                                new BingoGameDescription
                                {
                                    Patterns = new List<BingoPattern>
                                    {
                                        new(
                                            patternName2,
                                            patternId2,
                                            SideBetCardSerial,
                                            winAmountSideBet,
                                            ballQuantity2,
                                            paytableId2,
                                            false,
                                            bitFlags2,
                                            winIndex2)
                                    },
                                    GameEndWinEligibility = 0,
                                    GameIndex = 1
                                },
                            }
                        }
                    });

            var bingoDetails = new GameOutcomeBingoDetails(
                1,
                new CardPlayed[] { new(CardSerial, int.MaxValue, false) },
                ballCall.ToList(),
                0);
            var bingoDetailsSideBet = new GameOutcomeBingoDetails(
                0,
                new CardPlayed[] { new(SideBetCardSerial, int.MaxValue, false) },
                ballCall.ToList(),
                0);
            var gameDetails = new GameOutcomeGameDetails("0", 123, 123, 1, "Test Paytable", gameSerial);
            var gameDetailsSideBet = new GameOutcomeGameDetails("1", 4, 5, 1, "Side Bet Paytable", gameSerial);
            var winDetails = new GameOutcomeWinDetails(
                winAmount,
                string.Empty,
                new WinResult[]
                {
                    new(patternId, winAmount, ballQuantity, int.MaxValue, paytableId, patternName, (int)CardSerial, false, winIndex, Enumerable.Empty<string>()),
                });
            var winDetailsSideBet = new GameOutcomeWinDetails(
                winAmountSideBet,
                string.Empty,
                new WinResult[]
                {
                    new(patternId2, winAmountSideBet, ballQuantity2, int.MaxValue, paytableId2, patternName2, (int)SideBetCardSerial, false, winIndex2, Enumerable.Empty<string>())
                });
            var outcome = new GameOutcome(ResponseCode.Ok, winDetails, gameDetails, bingoDetails, true, false, 0, 0);
            var outcomeSideBet = new GameOutcome(ResponseCode.Ok, winDetailsSideBet, gameDetailsSideBet, bingoDetailsSideBet, true, false, 1, 1);
            var outcomes = new GameOutcomes(ResponseCode.Ok, new List<GameOutcome> { outcome, outcomeSideBet });

            await _target.ProcessGameOutcomes(outcomes, token);
        }
    }
}