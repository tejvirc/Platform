namespace Aristocrat.Monaco.Hhr.UI.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Linq;
    using Accounting.Contracts;
    using Application.Contracts;
    using Gaming.Contracts;
    using Gaming.Contracts.Progressives;
    using Gaming.Contracts.Progressives.Linked;
    using Hhr.Services;
    using Hhr.UI.ViewModels;
    using Kernel;
    using Menu;
    using Storage.Helpers;
    using Test.Common;
    using Views;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class HostPageViewModelManagerTest
    {
        private HostPageViewModelManager _target;
        private Mock<IPrizeDeterminationService> _prizeDeterminationService;
        private Mock<IPropertiesManager> _properties;
        private Mock<ISystemDisableManager> _systemDisable;
        private Mock<IEventBus> _eventBus;
        private Mock<IBank> _bank;
        private Mock<IGameDataService> _gameDataService;
        private Mock<IGameProvider> _gameProvider;
        private Mock<IProtocolLinkedProgressiveAdapter> _protocolLinkedProgressiveAdapter;
        private Mock<IPrizeInformationEntityHelper> _prizeInformationEntityHelper;
        private Mock<IManualHandicapEntityHelper> _manualHandicapEntityHelper;
        private Mock<IPlayerBank> _playerBank;
        private Mock<IRuntimeFlagHandler> _runtimeFlagHandler;
        private Mock<IHhrHostPageView> _hhrHostPageView;
        private Mock<ITransactionCoordinator> _transactionCoordinator;

        private ManualHandicapPageViewModel _manualHandicapPageViewModel;
        private RaceStatsPageViewModel _raceStatsPageViewModel;
        private WinningCombinationPageViewModel _winningCombinationPageViewModel;
        private CurrentProgressivePageViewModel _currentProgressivePageViewModel;
        private PreviousRaceResultPageViewModel _previousRaceResultPageViewModel;
        private ManualHandicapHelpPageViewModel _manualHandicapHelpPageViewModel;
        private HelpPageViewModel _helpPageViewModel;
        private BetHelpPageViewModel _betHelpPageViewModel;

        private readonly List<ProgressiveLevel> _progressiveLevels = new List<ProgressiveLevel>();

        [TestInitialize]
        public void TestInitialization()
        {
            MoqServiceManager.CreateInstance(MockBehavior.Default);
            _prizeInformationEntityHelper = new Mock<IPrizeInformationEntityHelper>(MockBehavior.Default);
            _manualHandicapEntityHelper = new Mock<IManualHandicapEntityHelper>(MockBehavior.Default);
            _prizeDeterminationService = new Mock<IPrizeDeterminationService>(MockBehavior.Default);
            _properties = new Mock<IPropertiesManager>(MockBehavior.Default);
            _eventBus = new Mock<IEventBus>(MockBehavior.Default);
            _systemDisable = new Mock<ISystemDisableManager>(MockBehavior.Default);
            _bank = new Mock<IBank>(MockBehavior.Default);
            _gameDataService = new Mock<IGameDataService>(MockBehavior.Default);
            _gameProvider = new Mock<IGameProvider>(MockBehavior.Default);
            _playerBank = new Mock<IPlayerBank>(MockBehavior.Default);
            _runtimeFlagHandler = new Mock<IRuntimeFlagHandler>(MockBehavior.Default);
            _transactionCoordinator = new Mock<ITransactionCoordinator>(MockBehavior.Default);

            _protocolLinkedProgressiveAdapter =
                MoqServiceManager.CreateAndAddService<IProtocolLinkedProgressiveAdapter>(MockBehavior.Strict);

            _protocolLinkedProgressiveAdapter.Setup(l => l.ViewLinkedProgressiveLevels())
                .Returns(new List<LinkedProgressiveLevel>());

            _properties.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>())).Returns(It.IsAny<int>());

            _properties.Setup(p =>
                    p.GetProperty(GamingConstants.ProgressivePoolCreationType, ProgressivePoolCreation.Default))
                .Returns(ProgressivePoolCreation.WagerBased);

            _transactionCoordinator.Setup(t => t.RequestTransaction(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<TransactionType>())).Returns(Guid.NewGuid());

            _hhrHostPageView = new Mock<IHhrHostPageView>();

            if (Application.Current == null)
            {
                // ReSharper disable once ObjectCreationAsStatement
                new Application();
            }

            _manualHandicapPageViewModel = new ManualHandicapPageViewModel(
                _properties.Object,
                _eventBus.Object,
                _systemDisable.Object,
                _prizeDeterminationService.Object,
                _manualHandicapEntityHelper.Object,
                _gameProvider.Object);

            _manualHandicapHelpPageViewModel = new ManualHandicapHelpPageViewModel(
                _eventBus.Object,
                _bank.Object,
                _gameProvider.Object,
                _properties.Object);

            _raceStatsPageViewModel = new RaceStatsPageViewModel(
                _properties.Object,
                _prizeDeterminationService.Object,
                _eventBus.Object);

            _winningCombinationPageViewModel = new WinningCombinationPageViewModel(
                _gameDataService.Object,
                _gameProvider.Object,
                _properties.Object,
                _eventBus.Object,
                _systemDisable.Object);

            _previousRaceResultPageViewModel = new PreviousRaceResultPageViewModel(_prizeInformationEntityHelper.Object);
            _helpPageViewModel = new HelpPageViewModel(_eventBus.Object);
            _betHelpPageViewModel = new BetHelpPageViewModel(
                _eventBus.Object,
                _properties.Object,
                _protocolLinkedProgressiveAdapter.Object);

            _currentProgressivePageViewModel = CreateCurrentProgressivePageViewModel();

            _target = new HostPageViewModelManager(_eventBus.Object,
                _manualHandicapPageViewModel,
                _raceStatsPageViewModel,
                _winningCombinationPageViewModel,
                _manualHandicapHelpPageViewModel,
                _previousRaceResultPageViewModel,
                _currentProgressivePageViewModel,
                _helpPageViewModel,
                _betHelpPageViewModel,
                _playerBank.Object,
                _runtimeFlagHandler.Object,
                _properties.Object,
                _transactionCoordinator.Object
                );

            _target.SetView(_hhrHostPageView.Object);

            CreateProgressiveLevels(2, 1, 1, 1,
                1000, ProtocolNames.HHR, 12345000, LevelCreationType.All);

            _protocolLinkedProgressiveAdapter.Setup(x => x.GetActiveProgressiveLevels()).Returns(_progressiveLevels);

        }

        [TestCleanup]
        public void TestCleanup()
        {
            MoqServiceManager.RemoveInstance();
        }

        [TestMethod]
        public void PreviouResultPage_ClickOnHelp_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Help).Execute(Command.Help);
            Assert.IsTrue(_target.SelectedViewModel is HelpPageViewModel);
        }

        [TestMethod]
        public void PreviousResultPage_ClickOnReturnToGame_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ReturnToGame).Execute(Command.ReturnToGame);
            Assert.IsTrue(_target.SelectedViewModel is null);
        }

        [TestMethod]
        public void PreviousResultPage_ClickOnManualHandicap_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ManualHandicap).Execute(Command.ManualHandicap);
            Assert.IsTrue(_target.SelectedViewModel is ManualHandicapHelpPageViewModel);
        }

        [TestMethod]
        public void ManualHandicapHelpPage_ClickOnReturnToGame_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ManualHandicap).Execute(Command.ManualHandicap);
            Assert.IsTrue(_target.SelectedViewModel is ManualHandicapHelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ReturnToGame).Execute(Command.ReturnToGame);
            Assert.IsTrue(_target.SelectedViewModel is null);
        }

        [TestMethod]
        public void ManualHandicapHelpPage_ClickOnHelp_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ManualHandicap).Execute(Command.ManualHandicap);
            Assert.IsTrue(_target.SelectedViewModel is ManualHandicapHelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Help).Execute(Command.Help);
            Assert.IsTrue(_target.SelectedViewModel is HelpPageViewModel);
        }

        private void SetupGame(long denomVal)
        {
            _properties.Setup(p => p.GetProperty(GamingConstants.SelectedGameId, It.IsAny<int>())).Returns(1);
            _properties.Setup(p => p.GetProperty(GamingConstants.IsGameRunning, It.IsAny<bool>())).Returns(true);
            _properties.Setup(p => p.GetProperty(GamingConstants.SelectedDenom, It.IsAny<long>())).Returns(denomVal);

            var denomination = new Mock<IDenomination>();
            denomination.SetupGet(x => x.Value).Returns(denomVal);

            var gameInfoList = new List<IGameDetail>
            {
                new MockGameInfo {Denominations = new List<IDenomination> {denomination.Object}, Id = 1},
                new MockGameInfo {Denominations = new List<IDenomination> {denomination.Object}, Id = 2}
            };

            _properties.Setup(p => p.GetProperty(GamingConstants.Games, null)).Returns(gameInfoList);

            _gameProvider.Setup(m => m.GetGame(It.IsAny<int>())).Returns(gameInfoList[0]);
        }

        [DataRow(300, 100000, false, DisplayName = "Manual Handicap button returns to game --- not enough credits")]
        [DataRow(300, 300000, true, DisplayName = "Manual Handicap button ok  --- exactly enough credits")]
        [DataRow(300, 600000, true, DisplayName = "Manual Handicap button ok --- enough credits")]
        [DataTestMethod]
        public void ManualHandicapHelpPage_ClickOnManualHandicap_VerifySelectedViewModel(long betCreditAmount, long bankBalance, bool manualHandicapAllowed)
        {
            SetupGame(1000);

            _properties.Setup(p => p.GetProperty(GamingConstants.SelectedBetCredits, It.IsAny<long>())).Returns(betCreditAmount);

            _bank.Setup(m => m.QueryBalance()).Returns(bankBalance);

            _ = _target.Show(Command.PreviousResults);

            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ManualHandicap).Execute(Command.ManualHandicap);
            Assert.IsTrue(_target.SelectedViewModel is ManualHandicapHelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ManualHandicap).Execute(Command.ManualHandicap);

            if (manualHandicapAllowed)
            {
                Assert.IsTrue(_target.SelectedViewModel is ManualHandicapPageViewModel);
            }
            else
            {
                Assert.IsNull(_target.SelectedViewModel);
            }
        }

        [TestMethod]
        public void ManualHandicapPage_ClickOnStat_VerifySelectedViewModel()
        {
            SetupGame(1000L);

            _properties.Setup(p => p.GetProperty(GamingConstants.SelectedBetCredits, It.IsAny<long>())).Returns(300L);

            _bank.Setup(m => m.QueryBalance()).Returns(600000L);

            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ManualHandicap).Execute(Command.ManualHandicap);
            Assert.IsTrue(_target.SelectedViewModel is ManualHandicapHelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ManualHandicap).Execute(Command.ManualHandicap);
            Assert.IsTrue(_target.SelectedViewModel is ManualHandicapPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.RaceStats).Execute(Command.RaceStats);
            Assert.IsTrue(_target.SelectedViewModel is RaceStatsPageViewModel);
        }

        [TestMethod]
        public void HelpPage_ClickOnExitHelp_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Help).Execute(Command.Help);
            Assert.IsTrue(_target.SelectedViewModel is HelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ExitHelp).Execute(Command.ExitHelp);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);
        }

        [TestMethod]
        public void HelpPage_ClickOnNext_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Help).Execute(Command.Help);
            Assert.IsTrue(_target.SelectedViewModel is HelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Next).Execute(Command.Next);
            Assert.IsTrue(_target.SelectedViewModel is BetHelpPageViewModel);
        }

        [TestMethod]
        public void BetHelpPage_ClickOnExitHelp_VerifySelectedViewModel()
        {
            _protocolLinkedProgressiveAdapter.Setup(l => l.ViewLinkedProgressiveLevels())
                .Returns(new List<LinkedProgressiveLevel>());

            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Help).Execute(Command.Help);
            Assert.IsTrue(_target.SelectedViewModel is HelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Next).Execute(Command.Next);
            Assert.IsTrue(_target.SelectedViewModel is BetHelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.ExitHelp).Execute(Command.ExitHelp);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);
        }

        [TestMethod]
        public void BetHelpPage_ClickOnNext_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Help).Execute(Command.Help);
            Assert.IsTrue(_target.SelectedViewModel is HelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Next).Execute(Command.Next);
            Assert.IsTrue(_target.SelectedViewModel is BetHelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Next).Execute(Command.Next);
            Assert.IsTrue(_target.SelectedViewModel is CurrentProgressivePageViewModel);
        }

        [TestMethod]
        public void CurrentProgressivePage_ClickOnNext_VerifySelectedViewModel()
        {
            _ = _target.Show(Command.PreviousResults);
            Assert.IsTrue(_target.SelectedViewModel is PreviousRaceResultPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Help).Execute(Command.Help);
            Assert.IsTrue(_target.SelectedViewModel is HelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Next).Execute(Command.Next);
            Assert.IsTrue(_target.SelectedViewModel is BetHelpPageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Next).Execute(Command.Next);
            Assert.IsTrue(_target.SelectedViewModel is CurrentProgressivePageViewModel);

            _target.SelectedViewModel.Commands.First(c => c.Command == Command.Next).Execute(Command.Next);
            Assert.IsTrue(_target.SelectedViewModel is WinningCombinationPageViewModel);
        }

        private CurrentProgressivePageViewModel CreateCurrentProgressivePageViewModel(bool nullEventBus = false,
            bool nullLinkedProgressiveProvider = false)
        {
            return new CurrentProgressivePageViewModel(nullEventBus ? null : _eventBus.Object,
                nullLinkedProgressiveProvider ? null : _protocolLinkedProgressiveAdapter.Object);
        }

        private void CreateProgressiveLevels(int levelsToCreate, int progId,
            int progLevelId, int linkedLevelId,
            int linkedProgressiveGroupId, string protocolName, int wager,
            LevelCreationType levelCreationType = LevelCreationType.Default)
        {
            for (var i = 0; i < levelsToCreate; ++i)
            {
                var assignedProgressiveKey = $"{protocolName}, " +
                                             $"Level Id: {linkedLevelId + i}, " +
                                             $"Progressive Group Id: {linkedProgressiveGroupId + i}";

                _progressiveLevels.Add(new ProgressiveLevel
                {
                    ProgressiveId = progId + i,
                    LevelId = progLevelId + i,
                    AssignedProgressiveId = new AssignableProgressiveId(AssignableProgressiveType.Linked,
                        assignedProgressiveKey),
                    WagerCredits = wager,
                    ResetValue = (i + 1) * 10000,
                    CurrentValue = 100,
                    CreationType = levelCreationType
                });
            }
        }
    }
}
