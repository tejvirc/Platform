namespace Aristocrat.Monaco.Gaming.UI.Tests.ViewModels.OperatorMenu
{
    using System;
    using System.Collections.Generic;
    using Aristocrat.Monaco.Accounting.Contracts;
    using Aristocrat.Monaco.Application.Contracts;
    using Aristocrat.Monaco.Application.Contracts.OperatorMenu;
    using Aristocrat.Monaco.Common.Container;
    using Aristocrat.Monaco.Gaming.Contracts.Central;
    using Aristocrat.Monaco.Gaming.Contracts.Lobby;
    using Aristocrat.Monaco.Gaming.Contracts.Meters;
    using Aristocrat.Monaco.Gaming.Contracts.Models;
    using Aristocrat.Monaco.Gaming.Contracts.Progressives.Linked;
    using Aristocrat.Monaco.Gaming.Diagnostics;
    using Aristocrat.Monaco.Gaming.UI.ViewModels.OperatorMenu.DetailedGameMeters;
    using Aristocrat.Monaco.Hardware.Contracts.Cabinet;
    using Aristocrat.Monaco.Hardware.Contracts.IO;
    using Aristocrat.Monaco.Hardware.Contracts.Reel;
    using Contracts;
    using Contracts.Progressives;
    using Kernel;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using SimpleInjector;
    using Test.Common;
    using UI.ViewModels.OperatorMenu;
    using Vgt.Client12.Application.OperatorMenu;

    [TestClass]
    public class GameHistoryViewModelTests
    {
        private GameHistoryViewModel _target;
        private Mock<IGameDiagnostics> _gameDiagnostics = new Mock<IGameDiagnostics>(MockBehavior.Default);
        private Mock<IGameHistory> _gameHistory;
        private Mock<IGameRecovery> _gameRecovery = new Mock<IGameRecovery>(MockBehavior.Default);
        private Mock<IGamePlayState> _gamePlayState = new Mock<IGamePlayState>(MockBehavior.Default);
        private Mock<IMeterManager> _meterManager = new Mock<IMeterManager>(MockBehavior.Default);
        private Mock<IGameMeterManager> _gameMeterManager = new Mock<IGameMeterManager>(MockBehavior.Default);
        private Mock<ICurrencyInContainer> _currencyContainer = new Mock<ICurrencyInContainer>(MockBehavior.Default);
        private Mock<ILobbyStateManager> _lobbyStateManager = new Mock<ILobbyStateManager>(MockBehavior.Default);
        private Mock<IBank> _bank = new Mock<IBank>(MockBehavior.Default);
        private Mock<IProgressiveLevelProvider> _progressiveProvider = new Mock<IProgressiveLevelProvider>(MockBehavior.Default);
        private Mock<ICentralProvider> _centralProvider = new Mock<ICentralProvider>(MockBehavior.Default);
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter = new Mock<IProtocolLinkedProgressiveAdapter>(MockBehavior.Default);
        private Mock<IGameRoundMeterSnapshotProvider> _gameRoundMeterSnapshotProvider = new Mock<IGameRoundMeterSnapshotProvider>(MockBehavior.Default);
        private Mock<IReelController> _reelController = new Mock<IReelController>(MockBehavior.Default);
        private Mock<IGameRoundDetailsDisplayProvider> _gameRoundDetailsDisplayProvider;
        private Mock<IDialogService> _dialogService;
        private Mock<IContainerService> _containerService;
        private Mock<IPropertiesManager> _propertiesManager;
        private Mock<ISystemDisableManager> _disableManager = new Mock<ISystemDisableManager>(MockBehavior.Default);
        private Container _container;

        // base class mocks
        private Mock<IEventBus> _eventBus;
        private Mock<IOperatorMenuLauncher> _menuLauncher;
        private Mock<ICabinetDetectionService> _cabinet;
        private Mock<IIO> _iio;

        // this provides a default test application so we can access the Dispatcher
        private static System.Windows.Application _application;

        // This will point to the HandleEvent function
        private Action<TransactionCompletedEvent> _handleProtocolInitializedEvent = null;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _application = System.Windows.Application.Current ??
                new System.Windows.Application();
        }

        [TestInitialize]
        public virtual void TestInitialize()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _dialogService = MoqServiceManager.CreateAndAddService<IDialogService>(MockBehavior.Default);
            _gameRoundDetailsDisplayProvider = MoqServiceManager.CreateAndAddService<IGameRoundDetailsDisplayProvider>(MockBehavior.Default);
            _containerService = MoqServiceManager.CreateAndAddService<IContainerService>(MockBehavior.Loose);
            _propertiesManager = MoqServiceManager.CreateAndAddService<IPropertiesManager>(MockBehavior.Loose);
            _eventBus = MoqServiceManager.CreateAndAddService<IEventBus>(MockBehavior.Default);
            _menuLauncher = MoqServiceManager.CreateAndAddService<IOperatorMenuLauncher>(MockBehavior.Default);
            _gameHistory = MoqServiceManager.CreateAndAddService<IGameHistory>(MockBehavior.Default);
            _cabinet = MoqServiceManager.CreateAndAddService<ICabinetDetectionService>(MockBehavior.Default);
            _reelController = MoqServiceManager.CreateAndAddService<IReelController>(MockBehavior.Default);

            _iio = MoqServiceManager.CreateAndAddService<IIO>(MockBehavior.Default);
            MockLocalization.Setup(MockBehavior.Strict);

            _container = new Container();
            _container.AddResolveUnregisteredType(typeof(GameHistoryViewModelTests).FullName);
            
            _container.Register(() => _gameDiagnostics.Object, Lifestyle.Singleton);
            _container.Register(() => _gameHistory.Object, Lifestyle.Singleton);
            _container.Register(() => _gameRecovery.Object, Lifestyle.Singleton);
            _container.Register(() => _gamePlayState.Object, Lifestyle.Singleton);
            _container.Register(() => _lobbyStateManager.Object, Lifestyle.Singleton);
            _container.Register(() => _bank.Object, Lifestyle.Singleton);
            _container.Register(() => _progressiveProvider.Object, Lifestyle.Singleton);
            _container.Register(() => _propertiesManager.Object, Lifestyle.Singleton);
            _container.Register(() => _centralProvider.Object, Lifestyle.Singleton);
            _container.Register(() => _meterManager.Object, Lifestyle.Singleton);
            _container.Register(() => _gameMeterManager.Object, Lifestyle.Singleton);
            _container.Register(() => _currencyContainer.Object, Lifestyle.Singleton);
            _container.Register(() => _protocolLinkedProgressiveAdapter.Object, Lifestyle.Singleton);
            _container.Register(() => _disableManager.Object, Lifestyle.Singleton);
            _container.Register(() => _gameRoundMeterSnapshotProvider.Object, Lifestyle.Singleton);
            
            _containerService.Setup(m => m.Container).Returns(_container);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.MeterFreeGamesIndependently, false)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.ReplayPauseActive, true)).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.ReplayPauseEnable, true)).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.KeepGameRoundEvents, true)).Returns(true);
            _propertiesManager.Setup(m => m.GetProperty(ApplicationConstants.ReelControllerEnabled, false)).Returns(false);
            _eventBus.Setup(m => m.Subscribe(It.IsAny<GameHistoryViewModel>(), It.IsAny<Action<TransactionCompletedEvent>>()))
                .Callback<object, Action<TransactionCompletedEvent>>((_, action) => _handleProtocolInitializedEvent = action);

            _target = new GameHistoryViewModel();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _container.Dispose();
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void ReplayCommandTest()
        {
            _gameRecovery.Setup(m => m.IsRecovering).Returns(false);
            _gameHistory.Setup(m => m.GetByIndex(It.IsAny<int>()))
                .Returns(new GameHistoryLog(1));
            _menuLauncher.Setup(m => m.PreventExit());
            _gameDiagnostics.Setup(m => m.Start(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<ReplayContext>(), true))
                .Verifiable();
            SetupSelectedGame();

            _target.ReplayCommand.Execute(null);

            _gameDiagnostics.Verify(m => m.Start(
                It.IsAny<int>(), It.IsAny<long>(), It.IsAny<string>(), It.IsAny<ReplayContext>(), true), Times.Once);
        }

        [TestMethod]
        public void ShowGameMetersTest()
        {
            SetupSelectedGame();
            _dialogService.Setup(m => m.ShowInfoDialog<IOperatorMenuPage>(It.IsAny<GameHistoryViewModel>(), It.IsAny<DetailedGameMetersViewModel>(), It.IsAny<string>())).Verifiable();

            _target.ShowGameMetersCommand.Execute(null);

            _dialogService.Verify(m => m.ShowInfoDialog<IOperatorMenuPage>(It.IsAny<GameHistoryViewModel>(), It.IsAny<DetailedGameMetersViewModel>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void ShowGameTransactionsTest()
        {
            SetupSelectedGame();
            _dialogService.Setup(m => m.ShowInfoDialog<IOperatorMenuPage>(It.IsAny<GameHistoryViewModel>(), It.IsAny<DetailedGameMetersViewModel>(), It.IsAny<string>())).Verifiable();

            _target.ShowGameTransactionsCommand.Execute(null);

            _dialogService.Verify(m => m.ShowInfoDialog<IOperatorMenuPage>(It.IsAny<GameHistoryViewModel>(), It.IsAny<DetailedGameMetersViewModel>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void ShowGameProgressiveWinTest()
        {
            SetupSelectedGame();
            var progressiveLevel = new ProgressiveLevel
            {
                AssignedProgressiveId = new AssignableProgressiveId(AssignableProgressiveType.Linked, "P1"),
                LevelId = 1
            };

            var jackpotInfo = new JackpotInfo { LevelId = 1, PackName = "test", WagerCredits = 100 };
            var gameHistoryLog = new GameHistoryLog(1)
                                    {
                                        GameId = 123,
                                        LogSequence = 234,
                                        DenomId = 3,
                                        Jackpots = new List<JackpotInfo> { jackpotInfo }
                                    };
            _gameHistory.Setup(m => m.GetGameHistory())
                .Returns(new List<IGameHistoryLog> { gameHistoryLog });

            _progressiveProvider.Setup(m => m.GetProgressiveLevels("test", 123, 3, 100))
                .Returns(new List<ProgressiveLevel> { progressiveLevel });
            _propertiesManager.Setup(m => m.GetProperty("CurrencyMultiplier", null)).Returns(1.0);

            IViewableLinkedProgressiveLevel temp = null;
            _protocolLinkedProgressiveAdapter.Setup(m => m.ViewLinkedProgressiveLevel(It.IsAny<string>(), out temp))
                .Verifiable();

            _target.ShowGameProgressiveWinCommand.Execute(null);

            _protocolLinkedProgressiveAdapter.Verify(m => m.ViewLinkedProgressiveLevel(It.IsAny<string>(), out temp), Times.Once);
        }

        [TestMethod]
        public void ShowGameEventLogsTest()
        {
            SetupSelectedGame();
            _dialogService.Setup(m => m.ShowInfoDialog<IOperatorMenuPage>(It.IsAny<GameHistoryViewModel>(), It.IsAny<GameEventLogsViewModel>(), It.IsAny<string>())).Verifiable();

            _target.ShowGameEventLogsCommand.Execute(null);

            _dialogService.Verify(m => m.ShowInfoDialog<IOperatorMenuPage>(It.IsAny<GameHistoryViewModel>(), It.IsAny<GameEventLogsViewModel>(), It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        public void ShowGameDetailsTest()
        {
            SetupSelectedGame();

            var gameHistoryLog = new GameHistoryLog(1)
            {
                GameId = 123,
                LogSequence = 234,
                DenomId = 3,
                TransactionId = 234
            };
            _gameHistory.Setup(m => m.GetByIndex(It.IsAny<int>())).Returns(gameHistoryLog);
            _centralProvider.Setup(m => m.Transactions).Returns(new List<CentralTransaction> { new CentralTransaction { AssociatedTransactions = new List<long> { 234 }} });
            _gameRoundDetailsDisplayProvider.Setup(m => m.Display(It.IsAny<GameHistoryViewModel>(), _dialogService.Object, It.IsAny<string>(), It.IsAny<long>())).Verifiable();

            _target.ShowGameDetailsCommand.Execute(null);

            _gameRoundDetailsDisplayProvider.Verify(m => m.Display(It.IsAny<GameHistoryViewModel>(), _dialogService.Object, It.IsAny<string>(), It.IsAny<long>()), Times.Once);
        }

        [TestMethod]
        public void RefreshGameHistoryTest()
        {
            var gameDetail = new GameDetail { Id = 1, ThemeName = "TestTheme", VariationId = "99" };
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.AllGames, null))
                .Returns(new List<GameDetail> { gameDetail });

            var gameHistoryLog = new GameHistoryLog(1)
                {
                    GameId = 1,
                    PlayState = PlayState.Idle,
                    EndCredits = 1_00_000, // $1
                    FinalWin = 50_000,  // 50 cents
                    FinalWager = 10, // 10 cents
                    LogSequence = 2,
                    Transactions = new List<TransactionInfo> { new TransactionInfo { TransactionType = typeof(BillTransaction), TransactionId = 12, Amount = 10_00_000,  } }
                };
            _gameHistory.Setup(m => m.GetGameHistory())
                .Returns(new List<GameHistoryLog> { gameHistoryLog });
            _gameHistory.Setup(m => m.MaxEntries).Returns(1);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.AdditionalInfoGameInProgress, false)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);

            _target.LoadedCommand.Execute(null);

            // calls the HandleEvent method which calls RefreshGameHistory on the UI thread
            Assert.IsNotNull(_handleProtocolInitializedEvent);
            _handleProtocolInitializedEvent(null);

            Assert.AreEqual(1, _target.GameHistory.Count);
            Assert.AreEqual("TestTheme (99)", _target.GameHistory[0].GameName);
        }

        private void SetupSelectedGame()
        {
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.AdditionalInfoGameInProgress, false)).Returns(false);
            _propertiesManager.Setup(m => m.GetProperty(GamingConstants.IsGameRunning, false)).Returns(false);
            _target.SelectedGameItem = new GameRoundHistoryItem
            {
                ReplayIndex = 1,
                GameId = 123,
                DenomId = 1,
                RefNoText = "456",
                GameIndex = 2,
                LogSequence = 234,
                EndTime = DateTime.Now,
                TransactionId = 234
            };
        }
    }
}