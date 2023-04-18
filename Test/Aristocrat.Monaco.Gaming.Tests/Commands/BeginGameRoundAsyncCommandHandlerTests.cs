namespace Aristocrat.Monaco.Gaming.Tests.Commands
{
    using System.Linq;
    using System.Threading;
    using Contracts;
    using Contracts.Central;
    using Gaming.Commands;
    using Gaming.Progressives;
    using Gaming.Runtime;
    using Gaming.Runtime.Client;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class BeginGameRoundAsyncCommandHandlerTests
    {
        private Mock<IEventBus> _bus;
        private Mock<IGamePlayState> _gameState;
        private Mock<IGameRecovery> _recovery;
        private Mock<IPropertiesManager> _properties;
        private Mock<IRuntime> _runtime;
        private Mock<IGameDiagnostics> _gameDiagnostics;
        private Mock<IGameHistory> _gameHistory;
        private Mock<IProgressiveGameProvider> _progressiveGameProvider;
        private Mock<IGameStartConditionProvider> _gameStartConditionProvider;
        private BeginGameRoundAsyncCommandHandler _underTest;

        [TestInitialize]
        public void Setup()
        {
            _gameState = new Mock<IGamePlayState>();
            _recovery = new Mock<IGameRecovery>();
            _properties = new Mock<IPropertiesManager>();
            _bus = new Mock<IEventBus>();
            _runtime = new Mock<IRuntime>();
            _gameDiagnostics = new Mock<IGameDiagnostics>();
            _gameHistory = new Mock<IGameHistory>();
            _progressiveGameProvider = new Mock<IProgressiveGameProvider>();
            _gameStartConditionProvider = new Mock<IGameStartConditionProvider>();
            _underTest = new BeginGameRoundAsyncCommandHandler(
                _runtime.Object,
                _recovery.Object,
                _gameState.Object,
                _properties.Object,
                _gameDiagnostics.Object,
                _gameHistory.Object,
                _bus.Object,
                _progressiveGameProvider.Object,
                _gameStartConditionProvider.Object);
        }

        [TestMethod]
        public void GivenBeginGameRoundAsyncWhenRequestNullWagerCategoryExistsThenWagerCategoryIsSet()
        {
            var manualResetEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            uint wagerCategoryId = 100;
            var command = new BeginGameRoundAsync(
                1,
                1,
                1,
                new byte[] { 1 },
                null,
                (int)wagerCategoryId,
                Enumerable.Empty<IAdditionalGamePlayInfo>()
            );
            var game = new Mock<IGameDetail>(MockBehavior.Strict);
            var denom = new Mock<IDenomination>();
            var wagerCategory1 = new Mock<IWagerCategory>();
            wagerCategory1.Setup(x => x.Id).Returns("1");
            var wagerCategory2 = new Mock<IWagerCategory>();
            wagerCategory2.Setup(x => x.Id).Returns(wagerCategoryId.ToString());
            denom.Setup(x => x.Value).Returns(12345L);
            game.Setup(x => x.Id).Returns(1);
            game.Setup(x => x.Denominations).Returns(new [] { denom.Object });
            game.Setup(x => x.WagerCategories).Returns(new [] { wagerCategory1.Object, wagerCategory2.Object });

            _properties.Setup(x => x.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);

            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(12345L);
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(x => x.GetProperty(GamingConstants.Games, null)).Returns(new [] { game.Object });
            _recovery.Setup(x => x.IsRecovering).Returns(false);
            _gameDiagnostics.Setup(x => x.IsActive).Returns(true);
            _properties.Setup(x => x.SetProperty(GamingConstants.SelectedWagerCategory, wagerCategory2.Object))
                .Verifiable();
            _runtime.Setup(x => x.BeginGameRoundResponse(BeginGameRoundResult.Success, Enumerable.Empty<Outcome>(), null))
                .Callback(() => manualResetEvent.Set())
                .Verifiable();

            _underTest.Handle(command);

            manualResetEvent.WaitOne(5000);
            _properties.Verify();
            _runtime.Verify();
        }

        [TestMethod]
        public void GivenBeginGameRoundAsyncWhenRequestNullWagerCategoryNotFoundThenWagerCategorySuccess()
        {
            var manualResetEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
            var command = new BeginGameRoundAsync(
                1,
                1,
                1,
                new byte[] { 1 },
                null,
                0, // no wager category provided
                Enumerable.Empty<IAdditionalGamePlayInfo>()
            );
            var game = new Mock<IGameDetail>(MockBehavior.Strict);
            var denom = new Mock<IDenomination>();
            var wagerCategory1 = new Mock<IWagerCategory>();
            wagerCategory1.Setup(x => x.Id).Returns("1");
            var wagerCategory2 = new Mock<IWagerCategory>();
            wagerCategory2.Setup(x => x.Id).Returns("2");
            denom.Setup(x => x.Value).Returns(12345L);
            game.Setup(x => x.Id).Returns(1);
            game.Setup(x => x.Denominations).Returns(new [] { denom.Object });
            game.Setup(x => x.WagerCategories).Returns(new [] { wagerCategory1.Object, wagerCategory2.Object });

            _properties.Setup(x => x.GetProperty(GamingConstants.IsGameRunning, false)).Returns(true);

            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedDenom, 0L)).Returns(12345L);
            _properties.Setup(x => x.GetProperty(GamingConstants.SelectedGameId, 0)).Returns(1);
            _properties.Setup(x => x.GetProperty(GamingConstants.Games, null)).Returns(new [] { game.Object });
            _recovery.Setup(x => x.IsRecovering).Returns(false);
            _gameDiagnostics.Setup(x => x.IsActive).Returns(true);
            _properties.Setup(x => x.SetProperty(GamingConstants.SelectedWagerCategory, wagerCategory1.Object))
                .Verifiable();
            _runtime.Setup(x => x.BeginGameRoundResponse(BeginGameRoundResult.Success, Enumerable.Empty<Outcome>(), null))
                .Callback(() => manualResetEvent.Set())
                .Verifiable();

            _underTest.Handle(command);

            manualResetEvent.WaitOne(5000);
            _properties.Verify();
            _runtime.Verify();
        }
    }
}